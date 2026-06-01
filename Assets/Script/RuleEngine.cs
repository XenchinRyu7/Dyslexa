using UnityEngine;

public class RuleEngine : MonoBehaviour
{
    private const int MinDifficulty = 1;
    private const int MaxDifficulty = 5;

    public int CalculateChange(SessionMetrics metrics)
    {
        metrics.CalculateDerivedMetrics();

        return FuzzySugenoChange(
            metrics.accuracy,
            metrics.hint_rate,
            metrics.rata_waktu_respons
        );
    }

    public int CalculateDifficultyAfter(SessionMetrics metrics, int currentDifficulty)
    {
        int change = CalculateChange(metrics);
        return Mathf.Clamp(currentDifficulty + change, MinDifficulty, MaxDifficulty);
    }

    private int FuzzySugenoChange(float accuracy, float hintRate, float responseTime)
    {
        float accLow = Trapmf(accuracy, 0f, 0f, 0.45f, 0.60f);
        float accMedium = Trimf(accuracy, 0.45f, 0.70f, 0.87f);
        float accHigh = Trapmf(accuracy, 0.80f, 0.90f, 1f, 1f);

        float hintLow = Trapmf(hintRate, 0f, 0f, 0.10f, 0.20f);
        float hintHigh = Trapmf(hintRate, 0.15f, 0.30f, 1f, 1f);

        float responseFast = Trapmf(responseTime, 0f, 0f, 4f, 8f);
        float responseSlow = Trapmf(responseTime, 10f, 15f, 99f, 99f);

        float numerator = 0f;
        float denominator = 0f;

        AddRule(ref numerator, ref denominator, Mathf.Min(accHigh, hintLow), +1);
        AddRule(ref numerator, ref denominator, Mathf.Min(accHigh, Mathf.Min(hintLow, responseFast)), +1);
        AddRule(ref numerator, ref denominator, accLow, -1);
        AddRule(ref numerator, ref denominator, Mathf.Min(accLow, responseSlow), -1);
        AddRule(ref numerator, ref denominator, Mathf.Min(accMedium, hintLow), 0);
        AddRule(ref numerator, ref denominator, Mathf.Min(accMedium, hintHigh), -1);
        AddRule(ref numerator, ref denominator, Mathf.Min(accHigh, hintHigh), 0);

        if (denominator <= 0.0001f)
            return 0;

        float crisp = numerator / denominator;
        if (crisp >= 0.4f)
            return +1;

        if (crisp <= -0.4f)
            return -1;

        return 0;
    }

    private void AddRule(ref float numerator, ref float denominator, float weight, int output)
    {
        numerator += weight * output;
        denominator += weight;
    }

    private float Trimf(float x, float a, float b, float c)
    {
        if (x <= a || x >= c)
            return 0f;

        if (x <= b)
            return (x - a) / (b - a);

        return (c - x) / (c - b);
    }

    private float Trapmf(float x, float a, float b, float c, float d)
    {
        if (x < a || x > d)
            return 0f;

        if (Mathf.Approximately(a, b) && x <= b)
            return 1f;

        if (Mathf.Approximately(c, d) && x >= c)
            return 1f;

        if (x <= b)
            return (x - a) / (b - a);

        if (x <= c)
            return 1f;

        return (d - x) / (d - c);
    }
}
