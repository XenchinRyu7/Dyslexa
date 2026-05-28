using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class WorkingMemoryNumberPanel : MonoBehaviour, IHintable
{
    public Transform questionContainer;
    public Transform answerContainer;
    public float previewDelay = 0.45f;
    public float previewHold = 1.35f;
    public float submitDelay = 0.45f;

    private readonly Color normalColor = Color.white;
    private readonly Color pickedColor = new Color(0.48f, 0.86f, 1f, 1f);

    private Action<string> onAnswerSelected;
    private Question currentQuestion;
    private TextMeshProUGUI[] questionLabels;
    private Button[] answerButtons;
    private string[] targetSequence;
    private readonly List<string> playerSequence = new List<string>();
    private Coroutine previewRoutine;
    private bool acceptingInput;

    public void ShowQuestion(Question question, Action<string> callback)
    {
        currentQuestion = question;
        onAnswerSelected = callback;
        targetSequence = string.IsNullOrEmpty(question.correctAnswer)
            ? new string[0]
            : question.correctAnswer.Split('|');

        if (questionContainer == null)
            questionContainer = FindChildByName(transform, "ContainerQuestion");
        if (answerContainer == null)
            answerContainer = FindChildByName(transform, "ContainerAnswer");

        questionLabels = CollectQuestionLabels();
        answerButtons = CollectAnswerButtons();
        playerSequence.Clear();
        acceptingInput = false;

        SetupQuestionSlots();
        SetupAnswerButtons();
        StartPreview();
    }

    public void ShowHint()
    {
        if (currentQuestion == null) return;
        playerSequence.Clear();
        ResetAnswerColors();
        StartPreview();
    }

    public RectTransform GetCorrectButton()
    {
        if (targetSequence == null || targetSequence.Length == 0 || answerButtons == null) return null;

        foreach (Button button in answerButtons)
        {
            if (button != null && GetButtonValue(button) == targetSequence[0])
                return button.GetComponent<RectTransform>();
        }

        return null;
    }

    private void SetupQuestionSlots()
    {
        for (int i = 0; i < questionLabels.Length; i++)
        {
            bool hasTarget = i < targetSequence.Length;
            questionLabels[i].transform.parent.gameObject.SetActive(hasTarget);
            questionLabels[i].text = hasTarget ? targetSequence[i] : "";
        }
    }

    private void SetupAnswerButtons()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            Button button = answerButtons[i];
            if (button == null) continue;

            string value = (i + 1).ToString();
            SetButtonText(button, value);
            SetButtonColor(button, normalColor);
            button.interactable = false;
            button.onClick.RemoveAllListeners();

            Button capturedButton = button;
            string capturedValue = value;
            button.onClick.AddListener(() => OnAnswerClicked(capturedValue, capturedButton));
        }
    }

    private void StartPreview()
    {
        if (previewRoutine != null)
            StopCoroutine(previewRoutine);
        previewRoutine = StartCoroutine(PreviewSequence());
    }

    private IEnumerator PreviewSequence()
    {
        acceptingInput = false;
        SetAnswerButtonsInteractable(false);
        SetQuestionVisible(true);
        ResetAnswerColors();

        yield return new WaitForSeconds(previewHold);

        SetQuestionVisible(false);
        yield return new WaitForSeconds(previewDelay);

        acceptingInput = true;
        SetAnswerButtonsInteractable(true);
    }

    private void OnAnswerClicked(string value, Button button)
    {
        if (!acceptingInput) return;
        if (playerSequence.Count >= targetSequence.Length) return;

        int slotIndex = playerSequence.Count;
        playerSequence.Add(value);
        if (slotIndex < questionLabels.Length)
            questionLabels[slotIndex].text = value;

        SetButtonColor(button, pickedColor);

        if (playerSequence.Count >= targetSequence.Length)
        {
            acceptingInput = false;
            SetAnswerButtonsInteractable(false);
            StartCoroutine(SubmitAfterDelay(string.Join("|", playerSequence)));
        }
    }

    private IEnumerator SubmitAfterDelay(string answer)
    {
        yield return new WaitForSeconds(submitDelay);
        onAnswerSelected?.Invoke(answer);
    }

    private void SetQuestionVisible(bool visible)
    {
        for (int i = 0; i < questionLabels.Length; i++)
            questionLabels[i].text = visible && i < targetSequence.Length ? targetSequence[i] : "";
    }

    private void SetAnswerButtonsInteractable(bool interactable)
    {
        foreach (Button button in answerButtons)
        {
            if (button != null && button.gameObject.activeSelf)
                button.interactable = interactable;
        }
    }

    private void ResetAnswerColors()
    {
        foreach (Button button in answerButtons)
        {
            if (button != null)
                SetButtonColor(button, normalColor);
        }
    }

    private TextMeshProUGUI[] CollectQuestionLabels()
    {
        if (questionContainer == null)
            return new TextMeshProUGUI[0];

        List<TextMeshProUGUI> result = new List<TextMeshProUGUI>();
        Transform[] slots = CollectDockChildren(questionContainer);
        foreach (Transform slot in slots)
        {
            TextMeshProUGUI label = slot.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
                result.Add(label);
        }

        return result.ToArray();
    }

    private Button[] CollectAnswerButtons()
    {
        if (answerContainer == null)
            return new Button[0];

        List<Button> result = new List<Button>();
        Transform[] slots = CollectDockChildren(answerContainer);
        foreach (Transform slot in slots)
        {
            Button button = slot.GetComponent<Button>();
            if (button != null)
                result.Add(button);
        }

        return result.ToArray();
    }

    private Transform[] CollectDockChildren(Transform parent)
    {
        List<Transform> result = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.StartsWith("Dock", StringComparison.OrdinalIgnoreCase))
                result.Add(child);
        }

        result.Sort((a, b) => GetDockNumber(a).CompareTo(GetDockNumber(b)));
        return result.ToArray();
    }

    private int GetDockNumber(Transform item)
    {
        string digits = "";
        foreach (char c in item.name)
        {
            if (char.IsDigit(c))
                digits += c;
        }

        return int.TryParse(digits, out int value) ? value : int.MaxValue;
    }

    private Transform FindChildByName(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }

    private string GetButtonValue(Button button)
    {
        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        return label != null ? label.text : "";
    }

    private void SetButtonText(Button button, string value)
    {
        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
            label.text = value;
    }

    private void SetButtonColor(Button button, Color color)
    {
        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = color;
    }
}
