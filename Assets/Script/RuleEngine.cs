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

    /// <summary>
    /// PURE CALCULATION — tidak mengubah state apapun.
    /// Gunakan ini untuk perbandingan/log tanpa side effect.
    /// </summary>
    public int CalculateChange(SessionMetrics metrics)
    {
        if (metrics.accuracy >= 0.85f && metrics.hint_rate < 0.2f) return +1;
        if (metrics.accuracy <  0.60f)                              return -1;
        return 0;
    }

    /// <summary>
    /// APPLY — kalkulasi DAN simpan hasilnya ke ProgressManager.
    /// Panggil ini hanya kalau Rule Engine yang dipakai (bukan ML).
    /// </summary>
    public void EvaluateAndAdapt(SessionMetrics metrics)
    {
        metrics.CalculateDerivedMetrics();

        int currentDifficulty = ProgressManager.Instance.GetCurrentDifficulty();
        float phonologyWeight = ProgressManager.Instance.GetPhonologyWeight();
        float visualWeight    = ProgressManager.Instance.GetVisualWeight();
        int   difficultyBefore = currentDifficulty;

        // Difficulty adjustment
        currentDifficulty += CalculateChange(metrics);
        currentDifficulty  = Mathf.Clamp(currentDifficulty, MIN_DIFFICULTY, MAX_DIFFICULTY);

        // Content weight adjustment
        if (metrics.kesalahan_fonologis > metrics.kesalahan_visual)
            phonologyWeight += 0.1f;
        else if (metrics.kesalahan_visual > metrics.kesalahan_fonologis)
            visualWeight += 0.1f;

        NormalizeWeights(ref phonologyWeight, ref visualWeight);

        // Save global state
        ProgressManager.Instance.SetCurrentDifficulty(currentDifficulty);
        ProgressManager.Instance.SetWeights(phonologyWeight, visualWeight);

        Debug.Log($"[RuleEngine] Difficulty: {difficultyBefore} → {currentDifficulty} (APPLIED)");
        Debug.Log($"[RuleEngine] Weights: Phonology={phonologyWeight:F2}, Visual={visualWeight:F2}");
    }

    /// <summary>
    /// Hanya update weights (untuk ML mode — difficulty diatur ML, tapi weights tetap diupdate).
    /// </summary>
    public void UpdateWeightsOnly(SessionMetrics metrics)
    {
        float phonologyWeight = ProgressManager.Instance.GetPhonologyWeight();
        float visualWeight    = ProgressManager.Instance.GetVisualWeight();

        if (metrics.kesalahan_fonologis > metrics.kesalahan_visual)
            phonologyWeight += 0.1f;
        else if (metrics.kesalahan_visual > metrics.kesalahan_fonologis)
            visualWeight += 0.1f;

        NormalizeWeights(ref phonologyWeight, ref visualWeight);
        ProgressManager.Instance.SetWeights(phonologyWeight, visualWeight);
        Debug.Log($"[RuleEngine] Weights updated (ML mode): Phonology={phonologyWeight:F2}, Visual={visualWeight:F2}");
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
