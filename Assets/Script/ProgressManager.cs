using UnityEngine;
using System.IO;

[System.Serializable]
public class GameProgress
{
    public int currentDifficulty = 1;
    public float phonologyWeight = 0.5f;
    public float visualWeight = 0.5f;
    public int currentUnlockedNode = 0;
    public int totalSessionsCompleted = 0;
    public float overallAccuracy = 0f;
}

public class ProgressManager : MonoBehaviour
{
    private static ProgressManager instance;
    private GameProgress progress;
    private string saveFilePath;

    public static ProgressManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ProgressManager");
                instance = go.AddComponent<ProgressManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeProgress();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void InitializeProgress()
    {
        // Path default sebelum profil dipilih — akan di-override oleh LoadForProfile()
        saveFilePath = Path.Combine(Application.persistentDataPath, "progress_default.json");
        progress = new GameProgress(); // kosong dulu, tunggu profil dipilih
    }

    /// <summary>
    /// Dipanggil saat profil dipilih. Load progress khusus profil tersebut.
    /// </summary>
    public void LoadForProfile(string profileId)
    {
        if (string.IsNullOrEmpty(profileId))
        {
            Debug.LogWarning("[ProgressManager] profileId kosong, pakai default.");
            return;
        }
        saveFilePath = Path.Combine(Application.persistentDataPath, $"progress_{profileId}.json");
        LoadProgress();
        Debug.Log($"[ProgressManager] Progress file: progress_{profileId}.json");
    }

    public void LoadProgress()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                progress = JsonUtility.FromJson<GameProgress>(json);
                Debug.Log($"[ProgressManager] Loaded progress: Difficulty={progress.currentDifficulty}, Unlocked={progress.currentUnlockedNode}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ProgressManager] Failed to load progress: {e.Message}");
                progress = new GameProgress();
            }
        }
        else
        {
            Debug.Log("[ProgressManager] No save file found, creating new progress");
            progress = new GameProgress();
            SaveProgress();
        }
    }

    public void SaveProgress()
    {
        try
        {
            string json = JsonUtility.ToJson(progress, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"[ProgressManager] Progress saved: Difficulty={progress.currentDifficulty}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ProgressManager] Failed to save progress: {e.Message}");
        }
    }

    public int GetCurrentDifficulty()
    {
        return progress.currentDifficulty;
    }

    public void SetCurrentDifficulty(int difficulty)
    {
        progress.currentDifficulty = Mathf.Clamp(difficulty, 1, 5);
        SaveProgress();
    }

    public float GetPhonologyWeight()
    {
        return progress.phonologyWeight;
    }

    public float GetVisualWeight()
    {
        return progress.visualWeight;
    }

    public void SetWeights(float phonology, float visual)
    {
        progress.phonologyWeight = phonology;
        progress.visualWeight = visual;
        SaveProgress();
    }

    public int GetCurrentUnlockedNode()
    {
        return progress.currentUnlockedNode;
    }

    public void SetCurrentUnlockedNode(int nodeIndex)
    {
        progress.currentUnlockedNode = nodeIndex;
        SaveProgress();
    }

    public void UpdateSessionStats(SessionMetrics metrics)
    {
        progress.totalSessionsCompleted++;
        
        // Calculate rolling average accuracy
        float totalAccuracy = progress.overallAccuracy * (progress.totalSessionsCompleted - 1);
        progress.overallAccuracy = (totalAccuracy + metrics.accuracy) / progress.totalSessionsCompleted;
        
        SaveProgress();
        
        Debug.Log($"[ProgressManager] Sessions: {progress.totalSessionsCompleted}, Overall Accuracy: {progress.overallAccuracy:P0}");
    }

    public GameProgress GetProgress()
    {
        return progress;
    }

    public void ResetProgress()
    {
        progress = new GameProgress();
        SaveProgress();
        Debug.Log("[ProgressManager] Progress reset to default");
    }
}
