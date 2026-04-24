using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// Panel Visual Spacing Awareness.
/// Difficulty mempengaruhi ukuran font opsi jawaban.
/// </summary>
public class VisualSpacingPanel : MonoBehaviour, IHintable
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
    private Question       currentQuestion;
    private Button[]       answerButtons;

    public void ShowQuestion(Question question, Action<string> callback)
    {
        onAnswerSelected = callback;
        currentQuestion  = question;
        answerButtons    = new Button[] { answer1, answer2, answer3 };

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

        float maxFont = answerFontMax[diffIdx];

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;

            // Reset warna button (kalau hint sebelumnya ngubah)
            answerButtons[i].image.color  = Color.white;
            answerButtons[i].interactable = true;

            TextMeshProUGUI label = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (label != null && i < question.options.Count)
            {
                label.text               = question.options[i];
                label.enableAutoSizing   = true;
                label.fontSizeMin        = 10f;
                label.fontSizeMax        = maxFont;
                label.enableWordWrapping = false;
            }

            answerButtons[i].onClick.RemoveAllListeners();
            int idx = i;
            answerButtons[i].onClick.AddListener(() =>
                onAnswerSelected?.Invoke(question.options[idx]));
        }
    }

    // ── HINT: eliminasi 1 opsi salah ──────────────────────
    public void ShowHint()
    {
        if (currentQuestion == null || answerButtons == null) return;

        List<int> wrongActive = new List<int>();
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null || !answerButtons[i].interactable) continue;
            TextMeshProUGUI lbl = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null && lbl.text != currentQuestion.correctAnswer)
                wrongActive.Add(i);
        }
        if (wrongActive.Count == 0) return;

        int pick = wrongActive[UnityEngine.Random.Range(0, wrongActive.Count)];
        answerButtons[pick].interactable = false;
        Image img = answerButtons[pick].GetComponent<Image>();
        if (img != null) img.color = new Color(1f, 0.25f, 0.25f, 0.6f); // merah = salah
        Debug.Log($"[Hint-Spacing] Eliminasi opsi: {pick}");
    }
}
