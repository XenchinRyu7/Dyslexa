using UnityEngine;
using System.IO;

[System.Serializable]
public class GameProgress
{
    // Progress per mode — Visual dan Fonologis independen
    public int unlockedNodeVisual    = 0;
    public int unlockedNodeFonologis = 0;
    public int unlockedNodeWorkingMemory = 0;
    public int difficultyVisual      = 1;
    public int difficultyFonologis   = 1;
    public int difficultyWorkingMemory = 1;

    // Data bersama
    public float phonologyWeight        = 0.5f;
    public float visualWeight           = 0.5f;
    public int   totalSessionsCompleted = 0;
    public float overallAccuracy        = 0f;

    // Bintang per level (maksimal 20 level per mode misalnya)
    public int[] nodeStarsVisual    = new int[20];
    public int[] nodeStarsFonologis = new int[20];
    public int[] nodeStarsWorkingMemory = new int[20];
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
                EnsureProgressDefaults();
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
            EnsureProgressDefaults();
            SaveProgress();
        }
    }

    private void EnsureProgressDefaults()
    {
        if (progress == null)
            progress = new GameProgress();
        if (progress.difficultyVisual < 1)
            progress.difficultyVisual = 1;
        if (progress.difficultyFonologis < 1)
            progress.difficultyFonologis = 1;
        if (progress.difficultyWorkingMemory < 1)
            progress.difficultyWorkingMemory = 1;
        if (progress.nodeStarsVisual == null || progress.nodeStarsVisual.Length < 20)
            progress.nodeStarsVisual = new int[20];
        if (progress.nodeStarsFonologis == null || progress.nodeStarsFonologis.Length < 20)
            progress.nodeStarsFonologis = new int[20];
        if (progress.nodeStarsWorkingMemory == null || progress.nodeStarsWorkingMemory.Length < 20)
            progress.nodeStarsWorkingMemory = new int[20];
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
        if (IsFonologis()) return progress.difficultyFonologis;
        if (IsWorkingMemory()) return progress.difficultyWorkingMemory;
        return progress.difficultyVisual;
    }

    public void SetCurrentDifficulty(int difficulty)
    {
        if (IsFonologis()) progress.difficultyFonologis = UnityEngine.Mathf.Clamp(difficulty, 1, 5);
        else if (IsWorkingMemory()) progress.difficultyWorkingMemory = UnityEngine.Mathf.Clamp(difficulty, 1, 5);
        else progress.difficultyVisual = UnityEngine.Mathf.Clamp(difficulty, 1, 5);
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

    public void UnlockNextNode(string mode, int currentNodeIndex)
    {
        if (mode == "Visual")
        {
            if (progress.unlockedNodeVisual <= currentNodeIndex)
                progress.unlockedNodeVisual = currentNodeIndex + 1;
        }
        else if (mode == "Fonologis")
        {
            if (progress.unlockedNodeFonologis <= currentNodeIndex)
                progress.unlockedNodeFonologis = currentNodeIndex + 1;
        }
        else if (mode == "WorkingMemory")
        {
            if (progress.unlockedNodeWorkingMemory <= currentNodeIndex)
                progress.unlockedNodeWorkingMemory = currentNodeIndex + 1;
        }
        SaveProgress();
    }

    /// <summary>
    /// Simpan bintang tertinggi yang pernah diraih di suatu level.
    /// </summary>
    public void SaveStars(string mode, int nodeIndex, int starsEarned)
    {
        if (nodeIndex < 0 || nodeIndex >= 20) return; // Asumsi max 20 level

        if (mode == "Visual")
        {
            if (starsEarned > progress.nodeStarsVisual[nodeIndex])
                progress.nodeStarsVisual[nodeIndex] = starsEarned;
        }
        else if (mode == "Fonologis")
        {
            if (starsEarned > progress.nodeStarsFonologis[nodeIndex])
                progress.nodeStarsFonologis[nodeIndex] = starsEarned;
        }
        else if (mode == "WorkingMemory")
        {
            if (starsEarned > progress.nodeStarsWorkingMemory[nodeIndex])
                progress.nodeStarsWorkingMemory[nodeIndex] = starsEarned;
        }
        SaveProgress();
    }

    public int GetStarsForNode(string mode, int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= 20) return 0;
        if (mode == "Visual") return progress.nodeStarsVisual[nodeIndex];
        if (mode == "WorkingMemory") return progress.nodeStarsWorkingMemory[nodeIndex];
        return progress.nodeStarsFonologis[nodeIndex];
    }

    private static bool IsFonologis()
        => UnityEngine.PlayerPrefs.GetString("SelectedGameMode", "Visual") == "Fonologis";

    private static bool IsWorkingMemory()
        => UnityEngine.PlayerPrefs.GetString("SelectedGameMode", "Visual") == "WorkingMemory";

    public int GetCurrentUnlockedNode()
    {
        if (IsFonologis()) return progress.unlockedNodeFonologis;
        if (IsWorkingMemory()) return progress.unlockedNodeWorkingMemory;
        return progress.unlockedNodeVisual;
    }

    public void SetCurrentUnlockedNode(int nodeIndex)
    {
        if (IsFonologis()) progress.unlockedNodeFonologis = nodeIndex;
        else if (IsWorkingMemory()) progress.unlockedNodeWorkingMemory = nodeIndex;
        else progress.unlockedNodeVisual = nodeIndex;
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
