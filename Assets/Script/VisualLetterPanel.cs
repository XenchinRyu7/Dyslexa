using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Panel Visual Letter Recognition.
/// Stimulus huruf = font 400 di difficulty 1, turun per difficulty.
/// Opsi jawaban = font 200 di difficulty 1, turun per difficulty.
/// </summary>
public class VisualLetterPanel : MonoBehaviour
{
    [Header("Stimulus")]
    public TextMeshProUGUI stimulusText;   // Question1/Text(TMP)

    [Header("Answer Buttons (fixed, 4 tombol)")]
    public Button answer1;
    public Button answer2;
    public Button answer3;
    public Button answer4;

    // Font size max stimulus per difficulty: 400 → 160
    private static readonly float[] stimulusFontMax = { 400f, 320f, 260f, 200f, 160f };

    // Font size max answer per difficulty: 200 → 80
    private static readonly float[] answerFontMax = { 200f, 160f, 130f, 100f, 80f };

    private Action<string> onAnswerSelected;

    public void ShowQuestion(Question question, Action<string> callback)
    {
        onAnswerSelected = callback;

        int diff    = ProgressManager.Instance.GetCurrentDifficulty();
        int diffIdx = Mathf.Clamp(diff - 1, 0, stimulusFontMax.Length - 1);

        // Stimulus huruf — besar di level mudah
        if (stimulusText != null)
        {
            stimulusText.text               = question.stimulus;
            stimulusText.enableAutoSizing   = true;
            stimulusText.fontSizeMin        = 40f;
            stimulusText.fontSizeMax        = stimulusFontMax[diffIdx];
            stimulusText.enableWordWrapping = false;
        }

        Button[] buttons = { answer1, answer2, answer3, answer4 };
        float maxFont = answerFontMax[diffIdx];

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            TextMeshProUGUI label = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (label != null && i < question.options.Count)
            {
                label.text               = question.options[i];
                label.enableAutoSizing   = true;
                label.fontSizeMin        = 10f;
                label.fontSizeMax        = maxFont;
                label.enableWordWrapping = false;
            }

            buttons[i].onClick.RemoveAllListeners();
            int idx = i;
            buttons[i].onClick.AddListener(() =>
                onAnswerSelected?.Invoke(question.options[idx]));
        }
    }
}
