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
    public Button btnRetry; // Tombol ulangi level

    [Header("UI Feedback (Stars & Audio)")]
    public GameObject starContainerObj; // Masukkan objek "StarContainer" ke sini
    public Unity.VectorGraphics.SVGImage[] starImages; // SVG Image star1, star2, star3
    public Sprite starFilled;       // Sprite bintang kuning/penuh
    public Sprite starEmpty;        // Sprite bintang abu-abu/kosong
    public AudioSource feedbackAudio; // "Speaker" SATU SAJA untuk memutar semua suara
    public AudioClip winSound;        // File audio "game_completed.wav"
    public AudioClip loseSound;       // File audio "game_over.mp3"
    public AudioClip correctSound;    // File audio "right.mp3" (benar per soal)
    public AudioClip wrongSound;      // File audio "wrong.mp3" (salah per soal)

    [Header("Hint")]
    public Button hintButton;                   // Tombol Bantuan
    public TextMeshProUGUI hintCountText;        // Teks "Bantuan (3)"
    public int maxHintsPerSession = 3;           // Max hint per session

    [Header("Pause Menu")]
    public GameObject pausePanel;
    public Button btnPause;             // Tombol Pause di pojok layar
    public Button btnPauseContinue;     // Tombol Lanjut di dalam panel
    public Button btnPauseRetry;        // Tombol Ulangi di dalam panel
    public Button btnPauseBackToMap;    // Tombol Keluar di dalam panel

    [Header("Panel Prefabs (instantiate saat soal muncul)")]
    public GameObject prefabVisualLetter;    // VisualLetter.prefab
    public GameObject prefabVisualSpacing;   // VisualSpacing.prefab
    public GameObject prefabBlending;        // PanelBlending.prefab
    public GameObject prefabSegmenting;      // PanelSegmenting.prefab
    public GameObject prefabWorkingMemory;   // PanelMemoryImage.prefab
    public GameObject prefabWorkingMemoryNumber; // PanelMemoryNumber.prefab

    [Header("Panel Container")]
    public Transform panelParent; // Canvas atau RectTransform tempat panel di-spawn

    // Instance aktif saat ini (di-destroy saat ganti soal)
    private GameObject activePanelInstance;

    [Header("Session Settings")]
    public int totalQuestions = 15;
    public int nodeIndex = 0;

    [Header("Managers")]
    private QuestionGenerator questionGenerator;
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

        if (btnRetry != null)
        {
            btnRetry.gameObject.SetActive(false);
            btnRetry.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        }

        // --- PAUSE MENU SETUP ---
        if (pausePanel != null) pausePanel.SetActive(false);
        if (btnPause != null) btnPause.onClick.AddListener(() => TogglePause(true));
        if (btnPauseContinue != null) btnPauseContinue.onClick.AddListener(() => TogglePause(false));
        
        if (btnPauseRetry != null) 
        {
            btnPauseRetry.onClick.AddListener(() => {
                Time.timeScale = 1f; // Wajib reset waktu sebelum load ulang
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
        
        if (btnPauseBackToMap != null) 
        {
            btnPauseBackToMap.onClick.AddListener(() => {
                Time.timeScale = 1f; // Wajib reset waktu sebelum pindah scene
                SceneManager.LoadScene("LevelMap");
            });
        }
        // ------------------------

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

        difficultyAtStart = ProgressManager.Instance.GetCurrentDifficulty();
        sessionStartTime  = Time.time;
    }

    void StartSession()
    {
        currentState = SessionState.Loading;


        questions = questionGenerator.GenerateQuestionSet(
            totalQuestions,
            ProgressManager.Instance.GetCurrentDifficulty(),
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
            case QuestionType.WorkingMemoryNumbers:      prefab = prefabWorkingMemoryNumber; break;
            case QuestionType.WorkingMemoryImages:       prefab = prefabWorkingMemory;       break;
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
            case QuestionType.WorkingMemoryNumbers:
                activePanelInstance.GetComponent<WorkingMemoryNumberPanel>()?.ShowQuestion(q, OnAnswerSelected);
                break;
            case QuestionType.WorkingMemoryImages:
                activePanelInstance.GetComponent<WorkingMemoryPanel>()?.ShowQuestion(q, OnAnswerSelected);
                break;
        }

        Debug.Log($"[GameSession] Panel spawned: {q.type}");

        // --- TUTORIAL HOOK ---
        if (TutorialManager.Instance != null)
        {
            StartCoroutine(RunTutorialSequence(q));
        }
    }

    private System.Collections.IEnumerator RunTutorialSequence(Question q)
    {
        yield return new WaitForSeconds(0.5f); // Tunggu UI render

        // 1. Pengenalan UI Global (Cuma 1x seumur hidup)
        if (!TutorialManager.Instance.IsTutorialCompleted("Tutorial_GameUI"))
        {
            var uiSteps = new List<TutorialStep>();
            if (progressContainer != null)
                uiSteps.Add(new TutorialStep { targetRect = progressContainer.GetComponent<RectTransform>(), text = "Isi bar ini buat menang!", requiresExactClick = false });
            if (hintButton != null)
                uiSteps.Add(new TutorialStep { targetRect = hintButton.GetComponent<RectTransform>(), text = "Klik tombol ini buat minta tolong!", requiresExactClick = false });
            if (btnPause != null)
                uiSteps.Add(new TutorialStep { targetRect = btnPause.GetComponent<RectTransform>(), text = "Klik ini buat jeda main.", requiresExactClick = false });
            
            TutorialManager.Instance.StartSequence("Tutorial_GameUI", uiSteps);
            
            // TUNGGU sampai tutorial UI selesai dimainkan oleh player, 
            // baru lanjut ngecek tutorial cara menjawab (mekanik).
            yield return new WaitWhile(() => TutorialManager.Instance.IsPlaying);
            // Tambah sedikit delay transisi
            yield return new WaitForSeconds(0.2f);
        }

        // 2. Pengenalan Cara Menjawab (Sesuai Mode)
        string modeKey = "Tutorial_Mechanic_" + q.type.ToString();
        if (!TutorialManager.Instance.IsTutorialCompleted(modeKey))
        {
            var answerSteps = new List<TutorialStep>();

            if (q.type == QuestionType.VisualLetterRecognition)
            {
                var targetBtn = activePanelInstance.GetComponent<VisualLetterPanel>()?.GetCorrectButton();
                if (targetBtn != null)
                    answerSteps.Add(new TutorialStep { targetRect = targetBtn, text = "Pilih huruf yang bener", requiresExactClick = true, onStepComplete = () => OnAnswerSelected(q.correctAnswer) });
            }
            else if (q.type == QuestionType.VisualSpacing)
            {
                var targetBtn = activePanelInstance.GetComponent<VisualSpacingPanel>()?.GetCorrectButton();
                if (targetBtn != null)
                    answerSteps.Add(new TutorialStep { targetRect = targetBtn, text = "Pilih kata yang spasinya pas", requiresExactClick = true, onStepComplete = () => OnAnswerSelected(q.correctAnswer) });
            }
            else if (q.type == QuestionType.PhonologyBlending)
            {
                var targetBtn = activePanelInstance.GetComponent<FonologisBlendingPanel>()?.GetCorrectButton();
                if (targetBtn != null)
                    answerSteps.Add(new TutorialStep { targetRect = targetBtn, text = "Dengar suaranya & pilih gambarnya", requiresExactClick = true, onStepComplete = () => OnAnswerSelected(q.correctAnswer) });
            }
            else if (q.type == QuestionType.PhonologySegmenting)
            {
                var panel = activePanelInstance.GetComponent<FonologisSegmentingPanel>();
                if (panel != null)
                {
                    // Ambil balok pertama dan slot pertama
                    RectTransform sourceRect = panel.syllable1?.GetComponent<RectTransform>();
                    RectTransform destRect = panel.slot1?.GetComponent<RectTransform>();
                    if (sourceRect != null && destRect != null)
                    {
                        answerSteps.Add(new TutorialStep { 
                            targetRect = sourceRect, 
                            dragTargetRect = destRect, 
                            text = "Tarik huruf ke kotak kosong!", 
                            requiresExactClick = false // Anak cuma klik overlay untuk nutup tutorial, lalu narik manual
                        });
                    }
                }
            }
            else if (QuestionTypeHelper.IsWorkingMemory(q.type))
            {
                RectTransform targetBtn = null;
                if (q.type == QuestionType.WorkingMemoryNumbers)
                    targetBtn = activePanelInstance.GetComponent<WorkingMemoryNumberPanel>()?.GetCorrectButton();
                else
                    targetBtn = activePanelInstance.GetComponent<WorkingMemoryPanel>()?.GetCorrectButton();

                if (targetBtn != null)
                {
                    string text = q.type == QuestionType.WorkingMemoryNumbers
                        ? "Ingat angka di atas, lalu tekan angka sesuai urutan!"
                        : "Ingat urutan gambar, lalu klik kartu sesuai urutan!";

                    answerSteps.Add(new TutorialStep { targetRect = targetBtn, text = text, requiresExactClick = false });
                }
            }

            if (answerSteps.Count > 0)
            {
                TutorialManager.Instance.StartSequence(modeKey, answerSteps);
            }
        }
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

        logger.LogQuestion(nodeIndex, ProgressManager.Instance.GetCurrentDifficulty(), currentQuestion.type, isCorrect, responseTime, false);

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

        // 2. Tampilkan feedback benar/salah (tanpa backToMapButton, btnRetry, dan tanpa bintang)
        if (feedbackPanel != null) feedbackPanel.SetActive(true);
        if (backToMapButton != null) backToMapButton.gameObject.SetActive(false); // hide saat mid-feedback
        if (btnRetry != null) btnRetry.gameObject.SetActive(false);               // hide saat mid-feedback
        if (starContainerObj != null) starContainerObj.SetActive(false);          // HIDE bintang di tengah game
        
        // 3. Mainkan Audio Benar/Salah (pakai speaker yang sama)
        if (feedbackAudio != null)
        {
            if (isCorrect && correctSound != null)
                feedbackAudio.PlayOneShot(correctSound);
            else if (!isCorrect && wrongSound != null)
                feedbackAudio.PlayOneShot(wrongSound);
        }

        if (feedbackText != null)
        {
            feedbackText.text  = isCorrect ? "Benar!" : "Salah";
            feedbackText.color = isCorrect ? Color.green : Color.red;
        }

        // 4. Tunggu 1 detik
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

        // ── PURE ML DDA ─────────────────────────────────────────
        int mlChange = 0;
        if (DyslexaMLInference.Instance != null)
        {
            mlChange = DyslexaMLInference.Instance.Predict(sessionMetrics, diffBefore);
        }
        else
        {
            Debug.LogWarning("[GameSession] DyslexaMLInference tidak ditemukan, fallback difficulty tetap 0.");
        }

        int diffAfter = Mathf.Clamp(diffBefore + mlChange, 1, 5);
        ProgressManager.Instance.SetCurrentDifficulty(diffAfter);

        // ── Content Weight Adjustment ──────────────────────────────
        float phonologyWeight = ProgressManager.Instance.GetPhonologyWeight();
        float visualWeight    = ProgressManager.Instance.GetVisualWeight();

        if (sessionMetrics.kesalahan_fonologis > sessionMetrics.kesalahan_visual)
            phonologyWeight += 0.1f;
        else if (sessionMetrics.kesalahan_visual > sessionMetrics.kesalahan_fonologis)
            visualWeight += 0.1f;

        // Normalize
        float total = phonologyWeight + visualWeight;
        if (total > 0)
        {
            phonologyWeight /= total;
            visualWeight /= total;
        }
        else
        {
            phonologyWeight = 0.5f;
            visualWeight = 0.5f;
        }

        ProgressManager.Instance.SetWeights(phonologyWeight, visualWeight);

        // ── Log ML Output ──────────────────────────────────────────
        Debug.Log($"[Session] ── Hasil Prediksi PURE ML ──────────────────────\n" +
                  $"  Accuracy        : {sessionMetrics.accuracy:P0}\n" +
                  $"  Hint rate       : {sessionMetrics.hint_rate:P0}\n" +
                  $"  Diff sebelum    : {diffBefore}\n" +
                  $"  ML Model Change : {mlChange:+0;-0;0}\n" +
                  $"  Diff sesudah    : {diffAfter}");


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

        // Tampilkan ringkasan di feedbackPanel + backToMapButton + btnRetry + Bintang
        if (feedbackPanel != null) feedbackPanel.SetActive(true);
        if (backToMapButton != null) backToMapButton.gameObject.SetActive(true); // show hanya di akhir session
        if (btnRetry != null) btnRetry.gameObject.SetActive(true);               // show di akhir session
        if (starContainerObj != null) starContainerObj.SetActive(true);          // SHOW bintang di akhir game
        if (progressContainer != null) progressContainer.gameObject.SetActive(false); // HIDE progress bar biar bersih

        float acc = sessionMetrics.accuracy;

        // Hitung jumlah bintang (0 sampai 3)
        int starCount = 0;
        if (acc >= 0.8f) starCount = 3;       // Akurasi >= 80% dapet 3 Bintang
        else if (acc >= 0.5f) starCount = 2;  // Akurasi >= 50% dapet 2 Bintang
        else if (acc > 0.0f) starCount = 1;   // Di bawah 50% dapet 1 Bintang

        // --- SIMPAN KE DATABASE / PROGRESS MANAGER ---
        // Biar nanti level map bisa tau dapet berapa bintang tertinggi di level ini
        ProgressManager.Instance.SaveStars(selectedMode, nodeIndex, starCount);

        // Update UI Bintang
        if (starImages != null && starImages.Length >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                // Kalau i kurang dari jumlah bintang, pakai gambar penuh. Kalau nggak, gambar kosong.
                starImages[i].sprite = (i < starCount) ? starFilled : starEmpty;
            }
        }

        // Mainkan Suara Kemenangan atau Kekalahan
        if (feedbackAudio != null)
        {
            if (starCount > 0 && winSound != null)
            {
                feedbackAudio.PlayOneShot(winSound);
            }
            else if (starCount == 0 && loseSound != null)
            {
                feedbackAudio.PlayOneShot(loseSound);
            }
        }

        if (feedbackText != null)
        {
            feedbackText.text = $"Benar: {sessionMetrics.jumlah_benar}/{sessionMetrics.total_soal}";
            feedbackText.color = Color.white; // Reset warna teks biar gak nyangkut merah/hijau dari soal terakhir
        }
        // backToMapButton sudah visible sejak Start() — tidak perlu show lagi di sini
    }

    public SessionMetrics GetSessionMetrics() => sessionMetrics;

    // =============================================
    // PAUSE MENU
    // =============================================
    public void TogglePause(bool isPaused)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
        
        // Hentikan waktu di Unity (0 = berhenti, 1 = normal)
        Time.timeScale = isPaused ? 0f : 1f;

        // --- Sembunyikan elemen lain saat Pause ---
        if (activePanelInstance != null)
        {
            activePanelInstance.SetActive(!isPaused); // Hilang pas pause, muncul pas lanjut
        }

        if (progressContainer != null)
        {
            progressContainer.gameObject.SetActive(!isPaused);
        }

        if (hintButton != null)
        {
            hintButton.gameObject.SetActive(!isPaused);
        }
        // ------------------------------------------
    }
}
