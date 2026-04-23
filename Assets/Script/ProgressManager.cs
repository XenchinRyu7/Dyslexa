using UnityEngine;
using System.IO;

[System.Serializable]
public class GameProgress
{
    // Progress per mode — Visual dan Fonologis independen
    public int unlockedNodeVisual    = 0;
    public int unlockedNodeFonologis = 0;
    public int difficultyVisual      = 1;
    public int difficultyFonologis   = 1;

    // Data bersama
    public float phonologyWeight        = 0.5f;
    public float visualWeight           = 0.5f;
    public int   totalSessionsCompleted = 0;
    public float overallAccuracy        = 0f;
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
                Debug.Log($"[ProgressManager] Loaded: V-diff={progress.difficultyVisual} F-diff={progress.difficultyFonologis} V-node={progress.unlockedNodeVisual} F-node={progress.unlockedNodeFonologis}");
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
            Debug.Log($"[ProgressManager] Progress saved.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ProgressManager] Failed to save progress: {e.Message}");
        }
    }

    public int GetCurrentDifficulty()
    {
        return IsFonologis() ? progress.difficultyFonologis : progress.difficultyVisual;
    }

    public void SetCurrentDifficulty(int difficulty)
    {
        if (IsFonologis()) progress.difficultyFonologis   = UnityEngine.Mathf.Clamp(difficulty, 1, 5);
        else               progress.difficultyVisual      = UnityEngine.Mathf.Clamp(difficulty, 1, 5);
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
        return IsFonologis() ? progress.unlockedNodeFonologis : progress.unlockedNodeVisual;
    }

    public void SetCurrentUnlockedNode(int nodeIndex)
    {
        if (IsFonologis()) progress.unlockedNodeFonologis = nodeIndex;
        else               progress.unlockedNodeVisual    = nodeIndex;
        SaveProgress();
    }

    private static bool IsFonologis()
        => UnityEngine.PlayerPrefs.GetString("SelectedGameMode", "Visual") == "Fonologis";

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
