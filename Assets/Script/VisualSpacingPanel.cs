using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Panel Visual Spacing Awareness.
/// Difficulty mempengaruhi ukuran font opsi jawaban.
/// </summary>
public class VisualSpacingPanel : MonoBehaviour
{
    [Header("Stimulus")]
    public TextMeshProUGUI stimulusText;  // Question/Text(TMP) — kata utuh

    [Header("Answer Buttons (fixed, 3 tombol)")]
    public Button answer1;
    public Button answer2;
    public Button answer3;

    // Font size max answer per difficulty: 120 → 40
    private static readonly float[] answerFontMax = { 120f, 96f, 76f, 60f, 46f };

    private Action<string> onAnswerSelected;

    public void ShowQuestion(Question question, Action<string> callback)
    {
        onAnswerSelected = callback;

        int diff    = ProgressManager.Instance.GetCurrentDifficulty();
        int diffIdx = Mathf.Clamp(diff - 1, 0, answerFontMax.Length - 1);

        // Stimulus kata selalu besar agar terbaca
        if (stimulusText != null)
        {
            stimulusText.text               = question.stimulus;
            stimulusText.enableAutoSizing   = true;
            stimulusText.fontSizeMin        = 20f;
            stimulusText.fontSizeMax        = 120f;
            stimulusText.enableWordWrapping = false;
        }

        Button[] buttons  = { answer1, answer2, answer3 };
        float    maxFont  = answerFontMax[diffIdx];

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
