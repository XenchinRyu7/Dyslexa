using System.Collections.Generic;
using UnityEngine;

public class QuestionGenerator : MonoBehaviour
{
    // Content weights (normalized, sum = 1)
    private float phonologyWeight = 0.5f;
    private float visualWeight = 0.5f;

    // Question banks loaded from JSON
    private QuestionBank phonologyBank;
    private QuestionBank visualBank;

    // Dynamic generator (optional)
    [Header("Generation Mode")]
    [Tooltip("Use dynamic generation for difficulty 3+ (infinite variability)")]
    public bool useDynamicGeneration = true;
    
    [Tooltip("Difficulty threshold to switch to dynamic (default: 3)")]
    public int dynamicGenerationThreshold = 3;

    private DynamicQuestionGenerator dynamicGenerator;
    private bool isInitialized = false;

    void Awake()
    {
        LoadQuestionBanks();
        
        // Initialize dynamic generator if enabled
        if (useDynamicGeneration)
        {
            dynamicGenerator = GetComponent<DynamicQuestionGenerator>();
            if (dynamicGenerator == null)
            {
                dynamicGenerator = gameObject.AddComponent<DynamicQuestionGenerator>();
                Debug.Log("[QuestionGenerator] Added DynamicQuestionGenerator component");
            }
        }
    }

    void LoadQuestionBanks()
    {
        // Load phonology questions from JSON
        TextAsset phonologyJson = Resources.Load<TextAsset>("phonology_questions");
        if (phonologyJson != null)
        {
            phonologyBank = JsonUtility.FromJson<QuestionBank>(phonologyJson.text);
            Debug.Log("[QuestionGenerator] Loaded phonology question bank");
        }
        else
        {
            Debug.LogError("[QuestionGenerator] Failed to load phonology_questions.json");
        }

        // Load visual questions from JSON
        TextAsset visualJson = Resources.Load<TextAsset>("visual_questions");
        if (visualJson != null)
        {
            visualBank = JsonUtility.FromJson<QuestionBank>(visualJson.text);
            Debug.Log("[QuestionGenerator] Loaded visual question bank");
        }
        else
        {
            Debug.LogError("[QuestionGenerator] Failed to load visual_questions.json");
        }

        isInitialized = (phonologyBank != null && visualBank != null);
    }

    public List<Question> GenerateQuestionSet(int totalQuestions, int difficulty, float phonologyW, float visualW)
    {
        if (!isInitialized)
        {
            Debug.LogError("[QuestionGenerator] Question banks not loaded!");
            return new List<Question>();
        }

        // Update weights
        phonologyWeight = phonologyW;
        visualWeight = visualW;

        // Calculate distribution
        int phonologyCount = Mathf.RoundToInt(totalQuestions * phonologyWeight);
        int visualCount = totalQuestions - phonologyCount;

        Debug.Log($"[QuestionGenerator] Weights - Phonology: {phonologyWeight:F2}, Visual: {visualWeight:F2}");
        Debug.Log($"[QuestionGenerator] Distribution - Phonology: {phonologyCount}, Visual: {visualCount}");

        List<Question> questions = new List<Question>();

        // Generate phonology questions
        for (int i = 0; i < phonologyCount; i++)
        {
            questions.Add(GeneratePhonologyQuestion(difficulty));
        }

        // Generate visual questions
        for (int i = 0; i < visualCount; i++)
        {
            questions.Add(GenerateVisualQuestion(difficulty));
        }

        // Shuffle the list
        ShuffleList(questions);

        Debug.Log($"[QuestionGenerator] Generated {questions.Count} questions (P:{phonologyCount}, V:{visualCount}) at difficulty {difficulty}");

        return questions;
    }

    private Question GeneratePhonologyQuestion(int difficulty)
    {
        // Use dynamic generation for higher difficulties
        if (useDynamicGeneration && dynamicGenerator != null && difficulty >= dynamicGenerationThreshold)
        {
            Debug.Log($"[QuestionGenerator] Using DYNAMIC phonology generation (difficulty {difficulty})");
            return dynamicGenerator.GeneratePhonologyQuestion(difficulty);
        }

        // Use JSON for lower difficulties
        List<QuestionData> questionPool = phonologyBank.GetQuestionsByDifficulty(difficulty);

        if (questionPool == null || questionPool.Count == 0)
        {
            Debug.LogWarning($"[QuestionGenerator] No phonology questions for difficulty {difficulty}");
            return GenerateFallbackQuestion(QuestionType.Phonology);
        }

        // Pick random question from pool
        QuestionData data = questionPool[Random.Range(0, questionPool.Count)];

        // Shuffle options so correct answer is not always first
        List<string> shuffledOptions = new List<string>(data.options);
        ShuffleList(shuffledOptions);

        return new Question(
            QuestionType.Phonology,
            data.stimulus,
            data.correctAnswer,
            shuffledOptions,
            data.audioClipName // Pass audio clip name
        );
    }

    private Question GenerateVisualQuestion(int difficulty)
    {
        // Use dynamic generation for higher difficulties
        if (useDynamicGeneration && dynamicGenerator != null && difficulty >= dynamicGenerationThreshold)
        {
            Debug.Log($"[QuestionGenerator] Using DYNAMIC visual generation (difficulty {difficulty})");
            return dynamicGenerator.GenerateVisualQuestion(difficulty);
        }

        // Use JSON for lower difficulties
        List<QuestionData> questionPool = visualBank.GetQuestionsByDifficulty(difficulty);

        if (questionPool == null || questionPool.Count == 0)
        {
            Debug.LogWarning($"[QuestionGenerator] No visual questions for difficulty {difficulty}");
            return GenerateFallbackQuestion(QuestionType.Visual);
        }

        // Pick random question from pool
        QuestionData data = questionPool[Random.Range(0, questionPool.Count)];

        // Shuffle options so correct answer is not always first
        List<string> shuffledOptions = new List<string>(data.options);
        ShuffleList(shuffledOptions);

        return new Question(
            QuestionType.Visual,
            data.stimulus,
            data.correctAnswer,
            shuffledOptions,
            "" // No audio for visual questions
        );
    }

    private Question GenerateFallbackQuestion(QuestionType type)
    {
        // Fallback question in case JSON loading fails
        if (type == QuestionType.Phonology)
        {
            List<string> options = new List<string> { "A", "E", "I", "O" };
            ShuffleList(options);
            
            return new Question(
                QuestionType.Phonology,
                "🔊 Dengarkan huruf yang disebutkan",
                "A",
                options,
                "Audio/sound_a" // Fallback audio
            );
        }
        else
        {
            List<string> options = new List<string> { "O", "I", "C", "Q" };
            ShuffleList(options);
            
            return new Question(
                QuestionType.Visual,
                "Pilih huruf 'O'",
                "O",
                options,
                "" // No audio for visual
            );
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}