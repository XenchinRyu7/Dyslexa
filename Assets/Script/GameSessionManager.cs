using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameSessionManager : MonoBehaviour
{
    [Header("Progress Bar")]
    public GameObject slotPrefab;
    public Transform progressContainer;

    [Header("Shared UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI feedbackText;
    public GameObject feedbackPanel;
    public Button backToMapButton;

    [Header("Hint")]
    public Button hintButton;                   // Tombol Bantuan
    public TextMeshProUGUI hintCountText;        // Teks "Bantuan (3)"
    public int maxHintsPerSession = 3;           // Max hint per session

    [Header("Panel Prefabs (instantiate saat soal muncul)")]
    public GameObject prefabVisualLetter;    // VisualLetter.prefab
    public GameObject prefabVisualSpacing;   // VisualSpacing.prefab
    public GameObject prefabBlending;        // PanelBlending.prefab
    public GameObject prefabSegmenting;      // PanelSegmenting.prefab

    [Header("Panel Container")]
    public Transform panelParent; // Canvas atau RectTransform tempat panel di-spawn

    // Instance aktif saat ini (di-destroy saat ganti soal)
    private GameObject activePanelInstance;

    [Header("Session Settings")]
    public int totalQuestions = 15;
    public int nodeIndex = 0;

    [Header("Managers")]
    private QuestionGenerator questionGenerator;
    private RuleEngine ruleEngine;
    private Logger logger;

    // Session state
    private SessionState currentState = SessionState.Loading;
    private List<Question> questions;
    private int currentQuestionIndex = 0;
    private Question currentQuestion;
    private string selectedMode;

    // Hint state
    private int  hintsRemaining;
    private bool hintUsedThisQuestion;

    // Metrics
    private SessionMetrics sessionMetrics;
    private float questionStartTime;
    private float sessionStartTime;
    private int difficultyAtStart;

    // Progress bar
    private List<Image> progressSlots = new List<Image>();
    private Color32 emptyColor  = new Color32(33, 39, 58, 255);
    private Color32 filledColor = new Color32(37, 116, 255, 255);

    void Start()
    {
        nodeIndex    = PlayerPrefs.GetInt("SelectedNodeIndex", 0);
        selectedMode = PlayerPrefs.GetString("SelectedGameMode", "Visual");

        // Auto-cari Canvas jika panelParent belum di-assign di Inspector
        if (panelParent == null)
        {
            Canvas c = FindObjectOfType<Canvas>();
            if (c != null)
            {
                panelParent = c.transform;
                Debug.LogWarning("[GameSession] panelParent null — auto-assigned ke Canvas: " + c.name);
            }
            else
                Debug.LogError("[GameSession] Tidak ada Canvas di scene! Panel tidak bisa di-spawn.");
        }

        InitializeManagers();
        InitializeProgressBar();
        InitializeMetrics();

        if (backToMapButton != null)
        {
            backToMapButton.gameObject.SetActive(false);
            backToMapButton.onClick.AddListener(() => SceneManager.LoadScene("LevelMap"));
        }

        // Hint setup — adaptif berdasarkan difficulty
        // Difficulty 1-2 → 3 hint (mudah, lebih banyak bantuan)
        // Difficulty 3   → 2 hint
        // Difficulty 4-5 → 1 hint (sulit, mandiri)
        int currentDiff = ProgressManager.Instance.GetCurrentDifficulty();
        hintsRemaining = currentDiff <= 2 ? 3 : currentDiff == 3 ? 2 : 1;
        hintUsedThisQuestion = false;
        UpdateHintUI();
        if (hintButton != null)
            hintButton.onClick.AddListener(OnHintClicked);

        StartSession();

        // BGM gameplay
        AudioManager.Instance?.PlayGameplay();
    }

    void Update()
    {
        if (currentState == SessionState.WaitingAnswer || currentState == SessionState.ShowingQuestion)
        {
            if (timerText != null)
            {
                float elapsed = Time.time - sessionStartTime;
                timerText.text = $"Waktu: {elapsed:F1}s";
            }
        }
    }

    void InitializeManagers()
    {
        questionGenerator = GetComponent<QuestionGenerator>() ?? gameObject.AddComponent<QuestionGenerator>();
        ruleEngine        = GetComponent<RuleEngine>()        ?? gameObject.AddComponent<RuleEngine>();
        logger            = GetComponent<Logger>()            ?? gameObject.AddComponent<Logger>();
    }

    void InitializeProgressBar()
    {
        if (slotPrefab == null || progressContainer == null) return;

        for (int i = 0; i < totalQuestions; i++)
        {
            GameObject slot = Instantiate(slotPrefab, progressContainer);
            Image img = slot.GetComponent<Image>();
            img.color = emptyColor;
            progressSlots.Add(img);
        }
    }

    void InitializeMetrics()
    {
        sessionMetrics = new SessionMetrics
        {
            total_soal          = totalQuestions,
            jumlah_benar        = 0,
            jumlah_salah        = 0,
            kesalahan_fonologis = 0,
            kesalahan_visual    = 0,
            penggunaan_hint     = 0,
            rata_waktu_respons  = 0f,
            waktu_penyelesaian  = 0f
        };

        difficultyAtStart = ruleEngine.GetCurrentDifficulty();
        sessionStartTime  = Time.time;
    }

    void StartSession()
    {
        currentState = SessionState.Loading;


        questions = questionGenerator.GenerateQuestionSet(
            totalQuestions,
            ruleEngine.GetCurrentDifficulty(),
            selectedMode
        );

        Debug.Log($"[GameSession] {questions.Count} soal di-generate. Mode: {selectedMode}");

        currentQuestionIndex = 0;
        ShowNextQuestion();
    }

    void ShowNextQuestion()
    {
        if (currentQuestionIndex >= questions.Count)
        {
            EndSession();
            return;
        }

        currentState    = SessionState.ShowingQuestion;
        currentQuestion = questions[currentQuestionIndex];


        if (feedbackPanel != null) feedbackPanel.SetActive(false);

        // Reset hint per soal
        hintUsedThisQuestion = false;

        SpawnPanelForQuestion(currentQuestion);

        questionStartTime = Time.time;
        currentState = SessionState.WaitingAnswer;
        UpdateHintUI(); // ← setelah state WaitingAnswer, baru update UI

        Debug.Log($"[GameSession] Soal {currentQuestionIndex + 1}/{totalQuestions} — Tipe: {currentQuestion.type}");
    }

    // =============================================
    // SPAWN PANEL DARI PREFAB
    // =============================================

    void SpawnPanelForQuestion(Question q)
    {
        // Destroy panel sebelumnya
        if (activePanelInstance != null)
            Destroy(activePanelInstance);

        // Pilih prefab yang sesuai
        GameObject prefab = null;
        switch (q.type)
        {
            case QuestionType.VisualLetterRecognition:  prefab = prefabVisualLetter;  break;
            case QuestionType.VisualSpacing:            prefab = prefabVisualSpacing; break;
            case QuestionType.PhonologyBlending:        prefab = prefabBlending;      break;
            case QuestionType.PhonologySegmenting:      prefab = prefabSegmenting;    break;
        }

        if (prefab == null)
        {
            Debug.LogError($"[GameSession] Prefab untuk tipe {q.type} belum di-assign di Inspector!");
            return;
        }

        // Instantiate ke panelParent (Canvas)
        activePanelInstance = Instantiate(prefab, panelParent);

        // Ambil script panel dan panggil ShowQuestion
        switch (q.type)
        {
            case QuestionType.VisualLetterRecognition:
                activePanelInstance.GetComponent<VisualLetterPanel>()?.ShowQuestion(q, OnAnswerSelected);
                break;
            case QuestionType.VisualSpacing:
                activePanelInstance.GetComponent<VisualSpacingPanel>()?.ShowQuestion(q, OnAnswerSelected);
                break;
            case QuestionType.PhonologyBlending:
                activePanelInstance.GetComponent<FonologisBlendingPanel>()?.ShowQuestion(q, OnAnswerSelected);
                break;
            case QuestionType.PhonologySegmenting:
                activePanelInstance.GetComponent<FonologisSegmentingPanel>()?.ShowQuestion(q, OnAnswerSelected);
                break;
        }

        Debug.Log($"[GameSession] Panel spawned: {q.type}");
    }

    // =============================================
    // HINT
    // =============================================

    public void OnHintClicked()
    {
        if (currentState != SessionState.WaitingAnswer) return;
        if (hintsRemaining <= 0 || hintUsedThisQuestion) return;
        if (activePanelInstance == null) return;

        IHintable hintable = activePanelInstance.GetComponent<IHintable>();
        if (hintable == null) { Debug.LogWarning("[Hint] Panel tidak implement IHintable!"); return; }

        hintable.ShowHint();
        hintsRemaining--;
        hintUsedThisQuestion = true;
        sessionMetrics.penggunaan_hint++;
        UpdateHintUI();
        Debug.Log($"[Hint] Digunakan. Sisa: {hintsRemaining}");
    }

    private void UpdateHintUI()
    {
        bool canHint = hintsRemaining > 0 && !hintUsedThisQuestion
                       && currentState == SessionState.WaitingAnswer;
        if (hintButton   != null) hintButton.interactable    = canHint;
        if (hintCountText != null) hintCountText.text        = $"{hintsRemaining}";
    }

    // =============================================
    // ANSWER HANDLING (dipanggil dari semua panel)
    // =============================================

    public void OnAnswerSelected(string answer)
    {
        if (currentState != SessionState.WaitingAnswer) return;

        currentState = SessionState.ShowingFeedback;

        float responseTime = Time.time - questionStartTime;
        bool isCorrect     = answer == currentQuestion.correctAnswer;

        // Update metrics
        if (isCorrect)
        {
            sessionMetrics.jumlah_benar++;
        }
        else
        {
            sessionMetrics.jumlah_salah++;
            if (QuestionTypeHelper.IsFonologis(currentQuestion.type))
                sessionMetrics.kesalahan_fonologis++;
            else
                sessionMetrics.kesalahan_visual++;
        }

        // Rolling average response time
        float total = sessionMetrics.rata_waktu_respons * currentQuestionIndex;
        sessionMetrics.rata_waktu_respons = (total + responseTime) / (currentQuestionIndex + 1);

        // Log
        // Map ke tipe lama untuk kompatibilitas Logger
        QuestionType logType = QuestionTypeHelper.IsFonologis(currentQuestion.type)
            ? QuestionType.PhonologyBlending
            : QuestionType.VisualLetterRecognition;

        logger.LogQuestion(nodeIndex, ruleEngine.GetCurrentDifficulty(), currentQuestion.type, isCorrect, responseTime, false);

        UpdateProgressBar();
        StartCoroutine(ShowFeedbackThenNext(isCorrect));
    }

    void UpdateProgressBar()
    {
        if (currentQuestionIndex < progressSlots.Count)
            progressSlots[currentQuestionIndex].color = filledColor;
    }

    IEnumerator ShowFeedbackThenNext(bool isCorrect)
    {
        // 1. Hide panel soal
        if (activePanelInstance != null)
            activePanelInstance.SetActive(false);

        // 2. Tampilkan feedback benar/salah (tanpa backToMapButton)
        if (feedbackPanel != null) feedbackPanel.SetActive(true);
        if (backToMapButton != null) backToMapButton.gameObject.SetActive(false); // hide saat mid-feedback
        if (feedbackText != null)
        {
            feedbackText.text  = isCorrect ? "Benar!" : "Salah";
            feedbackText.color = isCorrect ? Color.green : Color.red;
        }

        // 3. Tunggu 1 detik
        yield return new WaitForSeconds(1f);

        // 4. Hide feedback
        if (feedbackPanel != null) feedbackPanel.SetActive(false);

        // 5. Lanjut ke soal berikutnya (Destroy panel lama + spawn baru)
        currentQuestionIndex++;
        ShowNextQuestion();
    }

    // =============================================
    // END SESSION
    // =============================================

    void EndSession()
    {
        currentState = SessionState.Finished;
        sessionMetrics.waktu_penyelesaian = Time.time - sessionStartTime;
        sessionMetrics.CalculateDerivedMetrics();

        int diffBefore = difficultyAtStart;

        // ── Pure calculation Rule Engine (TANPA side effect) ─────────
        // CalculateChange() tidak mengubah ProgressManager sama sekali
        int ruleChange = ruleEngine.CalculateChange(sessionMetrics);

        // ── ML prediction (jika aktif & siap) ────────────────────────
        bool settingML  = SettingsWindowManager.UseML;
        bool instanceOK = DyslexaMLInference.Instance != null;
        bool useML      = settingML && instanceOK;

        int mlChange = useML
            ? DyslexaMLInference.Instance.Predict(sessionMetrics, diffBefore)
            : ruleChange;

        // ── Apply ke ProgressManager sesuai mode ─────────────────────
        int diffChange = useML ? mlChange : ruleChange;
        int diffAfter  = Mathf.Clamp(diffBefore + diffChange, 1, 5);

        if (useML)
        {
            // ML mode: difficulty dari ML, weights tetap diupdate dari error pattern
            ProgressManager.Instance.SetCurrentDifficulty(diffAfter);
            ruleEngine.UpdateWeightsOnly(sessionMetrics);
        }
        else
        {
            // Rule Engine mode: apply penuh (difficulty + weights)
            ruleEngine.EvaluateAndAdapt(sessionMetrics);
        }

        // ── Log perbandingan ─────────────────────────────────────────
        string activeMode = useML ? "ML ✅" : "Rule Engine";
        Debug.Log($"[Session] ── Hasil Prediksi ──────────────────────\n" +
                  $"  Mode aktif      : {activeMode}\n" +
                  $"  Accuracy        : {sessionMetrics.accuracy:P0}\n" +
                  $"  Hint rate       : {sessionMetrics.hint_rate:P0}\n" +
                  $"  Diff sebelum    : {diffBefore}\n" +
                  $"  Rule Engine     : {diffBefore} → {diffBefore + ruleChange} (change={ruleChange:+0;-0;0})\n" +
                  $"  ML Model        : {diffBefore} → {diffBefore + mlChange} (change={mlChange:+0;-0;0})\n" +
                  $"  DIPAKAI         : {(useML ? "ML" : "Rule Engine")}\n" +
                  (ruleChange != mlChange
                      ? $"  ⚡ BEDA! ML dan Rule Engine memberikan prediksi berbeda"
                      : $"  ✅ Sama — keduanya setuju"));


        ProgressManager.Instance.UpdateSessionStats(sessionMetrics);
        string pid   = PlayerProfileManager.Instance.ActiveProfile?.profileId ?? "unknown";
        string pname = PlayerProfileManager.Instance.ActiveProfile?.playerName ?? "unknown";
        logger.LogSession(pid, pname, nodeIndex, sessionMetrics, diffBefore, diffAfter);
        LevelMapGenerator.CheckAndUnlockNode(sessionMetrics);

        ShowResults(diffBefore, diffAfter);

        // Kembali ke BGM homescreen setelah session selesai
        AudioManager.Instance?.PlayHome();
    }

    void ShowResults(int diffBefore, int diffAfter)
    {
        if (activePanelInstance != null)
        {
            Destroy(activePanelInstance);
            activePanelInstance = null;
        }

        // Tampilkan ringkasan di feedbackPanel + backToMapButton
        if (feedbackPanel != null) feedbackPanel.SetActive(true);
        if (backToMapButton != null) backToMapButton.gameObject.SetActive(true); // show hanya di akhir session
        if (feedbackText != null)
        {
            float acc  = sessionMetrics.accuracy;
            string msg = acc >= 0.70f ? "Kerja Bagus!" : "Ayo Coba Lagi! Semangat";

            feedbackText.text =
                $"<b>{msg}</b>\n\n" +
                $"Benar: {sessionMetrics.jumlah_benar}/{sessionMetrics.total_soal}";
        }
        // backToMapButton sudah visible sejak Start() — tidak perlu show lagi di sini
    }

    public SessionMetrics GetSessionMetrics() => sessionMetrics;
}