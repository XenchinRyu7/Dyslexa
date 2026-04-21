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

    [Header("UI Elements")]
    public TextMeshProUGUI quizTitleText;
    public TextMeshProUGUI questionText;
    public Transform answerContainer;
    public GameObject answerButtonPrefab;
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI timerText;
    public Button backToMapButton;
    public Button playSoundButton; // NEW: Play audio for phonology questions
    public AudioSource audioSource; // NEW: Audio playback

    [Header("Session Settings")]
    public int totalQuestions = 15;
    public int nodeIndex = 0; // Set from LevelMap

    [Header("Managers")]
    private QuestionGenerator questionGenerator;
    private RuleEngine ruleEngine;
    private Logger logger;

    // Session state
    private SessionState currentState = SessionState.Loading;
    private List<Question> questions;
    private int currentQuestionIndex = 0;
    private Question currentQuestion;

    // Metrics tracking
    private SessionMetrics sessionMetrics;
    private float questionStartTime;
    private float sessionStartTime; // NEW: Track total session time
    private int difficultyAtStart;

    // Progress bar
    private List<Image> progressSlots = new List<Image>();
    private Color32 emptyColor = new Color32(33, 39, 58, 255);   // #21273A
    private Color32 filledColor = new Color32(37, 116, 255, 255); // #2574FF

    void Start()
    {
        // Get selected node index from PlayerPrefs
        nodeIndex = PlayerPrefs.GetInt("SelectedNodeIndex", 0);
        
        InitializeManagers();
        InitializeProgressBar();
        InitializeMetrics();
        
        // Setup back to map button
        if (backToMapButton != null)
        {
            backToMapButton.gameObject.SetActive(false); // Hidden initially
            backToMapButton.onClick.AddListener(() => SceneManager.LoadScene("LevelMap"));
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Setup play sound button
        if (playSoundButton != null)
        {
            playSoundButton.onClick.AddListener(PlayCurrentQuestionAudio);
        }
        
        StartSession();
    }

    void Update()
    {
        // Update timer display during gameplay
        if (currentState == SessionState.WaitingAnswer || currentState == SessionState.ShowingQuestion)
        {
            if (timerText != null)
            {
                float elapsedTime = Time.time - sessionStartTime;
                timerText.text = $"Waktu: {elapsedTime:F1}s";
            }
        }
    }

    void InitializeManagers()
    {
        // Get or add required components
        questionGenerator = GetComponent<QuestionGenerator>();
        if (questionGenerator == null)
            questionGenerator = gameObject.AddComponent<QuestionGenerator>();

        ruleEngine = GetComponent<RuleEngine>();
        if (ruleEngine == null)
            ruleEngine = gameObject.AddComponent<RuleEngine>();

        logger = GetComponent<Logger>();
        if (logger == null)
            logger = gameObject.AddComponent<Logger>();
    }

    void InitializeProgressBar()
    {
        if (slotPrefab == null || progressContainer == null)
        {
            Debug.LogWarning("[GameSession] Progress bar prefab or container not assigned!");
            return;
        }

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
            total_soal = totalQuestions,
            jumlah_benar = 0,
            jumlah_salah = 0,
            kesalahan_fonologis = 0,
            kesalahan_visual = 0,
            penggunaan_hint = 0,
            rata_waktu_respons = 0f,
            waktu_penyelesaian = 0f // NEW
        };

        difficultyAtStart = ruleEngine.GetCurrentDifficulty();
        sessionStartTime = Time.time;
    }

    void StartSession()
    {
        currentState = SessionState.Loading;

        if (quizTitleText != null)
            quizTitleText.text = $"Session {nodeIndex + 1}";

        // Cek mode apa yang dipilih pemain di scene ChooseMode
        float pWeight = ruleEngine.GetPhonologyWeight();
        float vWeight = ruleEngine.GetVisualWeight();

        string selectedMode = PlayerPrefs.GetString("SelectedGameMode", "Mixed");
        if (selectedMode == "Fonologis")
        {
            pWeight = 1.0f; // 100% soal fonologis
            vWeight = 0.0f;
            Debug.Log("[GameSession] Mode FONOLOGIS terpilih: Memaksa 100% soal fonologis.");
        }
        else if (selectedMode == "Visual")
        {
            pWeight = 0.0f;
            vWeight = 1.0f; // 100% soal visual
            Debug.Log("[GameSession] Mode VISUAL terpilih: Memaksa 100% soal visual.");
        }

        // Generate question set dengan weight yang sudah disesuaikan
        questions = questionGenerator.GenerateQuestionSet(
            totalQuestions,
            ruleEngine.GetCurrentDifficulty(),
            pWeight,
            vWeight
        );

        Debug.Log($"[GameSession] Generated {questions.Count} questions");

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

        currentState = SessionState.ShowingQuestion;
        currentQuestion = questions[currentQuestionIndex];

        // Update UI
        if (questionText != null)
            questionText.text = currentQuestion.stimulus;

        // Clear previous answer buttons
        foreach (Transform child in answerContainer)
        {
            Destroy(child.gameObject);
        }

        // Create answer buttons
        foreach (string option in currentQuestion.options)
        {
            GameObject btnObj = Instantiate(answerButtonPrefab, answerContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
                btnText.text = option;

            string selectedAnswer = option; // Capture for closure
            btn.onClick.AddListener(() => OnAnswerSelected(selectedAnswer));
        }

        // Hide feedback panel
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        // Show/hide play sound button based on question type
        if (playSoundButton != null)
        {
            if (currentQuestion.type == QuestionType.Phonology)
            {
                playSoundButton.gameObject.SetActive(true);
                // Auto-play audio for phonology questions
                PlayCurrentQuestionAudio();
            }
            else
            {
                playSoundButton.gameObject.SetActive(false);
            }
        }

        // Start timer
        questionStartTime = Time.time;
        currentState = SessionState.WaitingAnswer;

        Debug.Log($"[GameSession] Showing question {currentQuestionIndex + 1}/{totalQuestions}");
        Debug.Log($"[GameSession] Question type: {currentQuestion.type}");
    }

    public void OnAnswerSelected(string answer)
    {
        if (currentState != SessionState.WaitingAnswer)
            return;

        currentState = SessionState.ShowingFeedback;

        // Calculate response time
        float responseTime = Time.time - questionStartTime;

        // Check answer
        bool isCorrect = answer == currentQuestion.correctAnswer;

        // Update metrics
        if (isCorrect)
        {
            sessionMetrics.jumlah_benar++;
        }
        else
        {
            sessionMetrics.jumlah_salah++;

            // Track error types
            if (currentQuestion.type == QuestionType.Phonology)
                sessionMetrics.kesalahan_fonologis++;
            else
                sessionMetrics.kesalahan_visual++;
        }

        // Update average response time
        float totalTime = sessionMetrics.rata_waktu_respons * currentQuestionIndex;
        sessionMetrics.rata_waktu_respons = (totalTime + responseTime) / (currentQuestionIndex + 1);

        // Log question
        logger.LogQuestion(
            nodeIndex,
            ruleEngine.GetCurrentDifficulty(),
            currentQuestion.type,
            isCorrect,
            responseTime,
            false // usedHint - implement hint system later
        );

        // Update progress bar
        UpdateProgressBar();

        // Show feedback
        ShowFeedback(isCorrect);

        // Move to next question after delay
        StartCoroutine(NextQuestionAfterDelay(1.5f));
    }

    void UpdateProgressBar()
    {
        if (currentQuestionIndex < progressSlots.Count)
        {
            progressSlots[currentQuestionIndex].color = filledColor;
        }
    }

    void ShowFeedback(bool isCorrect)
    {
        if (feedbackPanel == null || feedbackText == null)
            return;

        feedbackPanel.SetActive(true);
        feedbackText.text = isCorrect ? "Benar!" : "Salah";
        feedbackText.color = isCorrect ? Color.green : Color.red;
    }

    IEnumerator NextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        currentQuestionIndex++;
        ShowNextQuestion();
    }

    void EndSession()
    {
        currentState = SessionState.Finished;

        // Calculate total session completion time
        sessionMetrics.waktu_penyelesaian = Time.time - sessionStartTime;

        Debug.Log("[GameSession] Session finished!");

        // Calculate final metrics
        sessionMetrics.CalculateDerivedMetrics();

        // Get difficulty before evaluation
        int difficultyBefore = ruleEngine.GetCurrentDifficulty();

        // Let RuleEngine evaluate and adapt (updates GLOBAL difficulty)
        ruleEngine.EvaluateAndAdapt(sessionMetrics);
        
        // Get difficulty after evaluation
        int difficultyAfter = ruleEngine.GetCurrentDifficulty();

        // Update ProgressManager with session stats
        ProgressManager.Instance.UpdateSessionStats(sessionMetrics);

        // Log session
        logger.LogSession(nodeIndex, sessionMetrics, difficultyBefore, difficultyAfter);

        // Check and unlock next node if mastery achieved
        LevelMapGenerator.CheckAndUnlockNode(sessionMetrics);

        // Show results
        ShowResults();
    }

    void ShowResults()
    {
        // Hide feedback panel if showing
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        // Clear answer buttons
        foreach (Transform child in answerContainer)
        {
            Destroy(child.gameObject);
        }

        // Show detailed results
        if (questionText != null)
        {
            string resultText = $"<size=40><b>SESSION SELESAI!</b></size>\n\n";
            resultText += $"<b>HASIL:</b>\n";
            resultText += $"✓ Benar: <color=green>{sessionMetrics.jumlah_benar}</color>/{sessionMetrics.total_soal}\n";
            resultText += $"✗ Salah: <color=red>{sessionMetrics.jumlah_salah}</color>\n";
            resultText += $"Akurasi: <b>{sessionMetrics.accuracy:P0}</b>\n\n";
            
            resultText += $"<b>DETAIL ERROR:</b>\n";
            resultText += $"Kesalahan Fonologis: {sessionMetrics.kesalahan_fonologis}\n";
            resultText += $"Kesalahan Visual: {sessionMetrics.kesalahan_visual}\n\n";
            
            resultText += $"<b>WAKTU:</b>\n";
            resultText += $"Total: {sessionMetrics.waktu_penyelesaian:F1}s\n";
            resultText += $"Rata-rata: {sessionMetrics.rata_waktu_respons:F1}s/soal\n\n";
            
            resultText += $"<b>DIFFICULTY:</b>\n";
            resultText += $"Before: {difficultyAtStart} → After: {ruleEngine.GetCurrentDifficulty()}";
            
            questionText.text = resultText;
        }

        if (feedbackPanel != null && backToMapButton != null)
        {
            feedbackPanel.SetActive(true);
            backToMapButton.gameObject.SetActive(true);
            
            if (feedbackText != null)
                feedbackText.gameObject.SetActive(false);
        }
    }

    void PlayCurrentQuestionAudio()
    {
        if (currentQuestion == null || audioSource == null)
            return;

        if (currentQuestion.type != QuestionType.Phonology)
            return;

        if (string.IsNullOrEmpty(currentQuestion.audioClipName))
        {
            Debug.LogWarning("[GameSession] No audio clip name for phonology question!");
            return;
        }

        // Load audio from Resources (audioClipName already includes path like "Audio/sound_b")
        AudioClip clip = Resources.Load<AudioClip>(currentQuestion.audioClipName);

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log($"[GameSession] Playing audio: {currentQuestion.audioClipName}");
        }
        else
        {
            Debug.LogWarning($"[GameSession] Audio not found: {currentQuestion.audioClipName}");
        }
    }

    public SessionMetrics GetSessionMetrics()
    {
        return sessionMetrics;
    }
}