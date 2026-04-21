using UnityEngine;

[System.Serializable]
public class SessionMetrics
{
    public int jumlah_benar;
    public int jumlah_salah;
    public float rata_waktu_respons;
    public int kesalahan_fonologis;
    public int kesalahan_visual;
    public int penggunaan_hint;
    public int total_soal;
    public float waktu_penyelesaian; // NEW: Total session completion time

    // Derived metrics
    public float accuracy;
    public float error_rate;
    public float hint_rate;
    public float fonologis_rate;
    public float visual_rate;

    public void CalculateDerivedMetrics()
    {
        accuracy = total_soal > 0 ? (float)jumlah_benar / total_soal : 0f;
        error_rate = total_soal > 0 ? (float)jumlah_salah / total_soal : 0f;
        hint_rate = total_soal > 0 ? (float)penggunaan_hint / total_soal : 0f;
        fonologis_rate = jumlah_salah > 0 ? (float)kesalahan_fonologis / jumlah_salah : 0f;
        visual_rate = jumlah_salah > 0 ? (float)kesalahan_visual / jumlah_salah : 0f;
    }
}

public class RuleEngine : MonoBehaviour
{
    private const int MIN_DIFFICULTY = 1;
    private const int MAX_DIFFICULTY = 5;

    public void EvaluateAndAdapt(SessionMetrics metrics)
    {
        metrics.CalculateDerivedMetrics();

        // Get current values from ProgressManager
        int currentDifficulty = ProgressManager.Instance.GetCurrentDifficulty();
        float phonologyWeight = ProgressManager.Instance.GetPhonologyWeight();
        float visualWeight = ProgressManager.Instance.GetVisualWeight();

        int difficultyBefore = currentDifficulty;

        // Difficulty adjustment rules
        if (metrics.accuracy >= 0.85f && metrics.hint_rate < 0.2f)
        {
            currentDifficulty++;
        }
        else if (metrics.accuracy < 0.6f)
        {
            currentDifficulty--;
        }
        // else: difficulty stays the same

        // Clamp difficulty between 1-5
        currentDifficulty = Mathf.Clamp(currentDifficulty, MIN_DIFFICULTY, MAX_DIFFICULTY);

        // Content weight adjustment
        if (metrics.kesalahan_fonologis > metrics.kesalahan_visual)
        {
            phonologyWeight += 0.1f;
        }
        else if (metrics.kesalahan_visual > metrics.kesalahan_fonologis)
        {
            visualWeight += 0.1f;
        }

        // Normalize weights so total = 1
        NormalizeWeights(ref phonologyWeight, ref visualWeight);

        // Save to ProgressManager (PERSISTENT GLOBAL STATE)
        ProgressManager.Instance.SetCurrentDifficulty(currentDifficulty);
        ProgressManager.Instance.SetWeights(phonologyWeight, visualWeight);

        Debug.Log($"[RuleEngine] Difficulty: {difficultyBefore} → {currentDifficulty} (GLOBAL)");
        Debug.Log($"[RuleEngine] Weights: Phonology={phonologyWeight:F2}, Visual={visualWeight:F2}");
    }

    private void NormalizeWeights(ref float phonology, ref float visual)
    {
        float total = phonology + visual;
        if (total > 0)
        {
            phonology /= total;
            visual /= total;
        }
        else
        {
            // Reset to default if somehow both are 0
            phonology = 0.5f;
            visual = 0.5f;
        }
    }

    public int GetCurrentDifficulty()
    {
        return ProgressManager.Instance.GetCurrentDifficulty();
    }

    public float GetPhonologyWeight()
    {
        return ProgressManager.Instance.GetPhonologyWeight();
    }

    public float GetVisualWeight()
    {
        return ProgressManager.Instance.GetVisualWeight();
    }
}
