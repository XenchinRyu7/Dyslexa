using UnityEngine;
using UnityEngine.InputSystem;

public class DebugHelper : MonoBehaviour
{
    [Header("Debug Controls (Keyboard)")]
    [Tooltip("P = Show progress | R = Reset | U = Unlock all | M = Max difficulty")]
    public string controls = "P/R/U/M";

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[Key.P].wasPressedThisFrame)
        {
            ShowProgress();
        }

        if (Keyboard.current[Key.R].wasPressedThisFrame)
        {
            ResetProgress();
        }

        if (Keyboard.current[Key.U].wasPressedThisFrame)
        {
            UnlockAllNodes();
        }

        if (Keyboard.current[Key.M].wasPressedThisFrame)
        {
            SetMaxDifficulty();
        }
    }

    void ShowProgress()
    {
        GameProgress progress = ProgressManager.Instance.GetProgress();
        
        Debug.Log("========== GAME PROGRESS ==========");
        Debug.Log($"Current Difficulty: {progress.currentDifficulty}/5");
        Debug.Log($"Phonology Weight: {progress.phonologyWeight:P0}");
        Debug.Log($"Visual Weight: {progress.visualWeight:P0}");
        Debug.Log($"Unlocked Node: {progress.currentUnlockedNode}");
        Debug.Log($"Sessions Completed: {progress.totalSessionsCompleted}");
        Debug.Log($"Overall Accuracy: {progress.overallAccuracy:P0}");
        Debug.Log($"Save Path: {Application.persistentDataPath}");
        Debug.Log("===================================");
    }

    void ResetProgress()
    {
        ProgressManager.Instance.ResetProgress();
        Debug.Log("<color=yellow>[DEBUG] Progress RESET to default!</color>");
        ShowProgress();
    }

    void UnlockAllNodes()
    {
        ProgressManager.Instance.SetCurrentUnlockedNode(9); // Unlock all 10 nodes (0-9)
        Debug.Log("<color=green>[DEBUG] All nodes UNLOCKED!</color>");
    }

    void SetMaxDifficulty()
    {
        ProgressManager.Instance.SetCurrentDifficulty(5);
        Debug.Log("<color=cyan>[DEBUG] Difficulty set to MAX (5)!</color>");
    }

    [ContextMenu("Show Current Progress")]
    void ShowProgressMenu()
    {
        ShowProgress();
    }

    [ContextMenu("Reset All Progress")]
    void ResetProgressMenu()
    {
        ResetProgress();
    }
}
