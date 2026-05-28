using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class WorkingMemoryPanel : MonoBehaviour, IHintable
{
    public Button card1;
    public Button card2;
    public Button card3;
    public Button card4;
    public Button card5;
    public Button card6;
    public Button card7;
    public Button card8;

    public float previewDelay = 0.45f;
    public float previewHold = 0.85f;
    public float submitDelay = 0.45f;

    private readonly Color hiddenColor = Color.white;
    private readonly Color previewColor = new Color(1f, 0.74f, 0.05f, 1f);
    private readonly Color pickedColor = new Color(0.48f, 0.86f, 1f, 1f);
    private readonly Color wrongColor = new Color(1f, 0.42f, 0.42f, 1f);

    private Action<string> onAnswerSelected;
    private Button[] cards;
    private List<string> options;
    private string[] targetSequence;
    private readonly List<string> playerSequence = new List<string>();
    private readonly HashSet<Button> pickedCards = new HashSet<Button>();
    private Coroutine previewRoutine;
    private bool acceptingInput;

    public void ShowQuestion(Question question, Action<string> callback)
    {
        onAnswerSelected = callback;
        cards = CollectCards();
        options = question.imageOptions;
        targetSequence = string.IsNullOrEmpty(question.correctAnswer)
            ? new string[0]
            : question.correctAnswer.Split('|');

        playerSequence.Clear();
        pickedCards.Clear();
        acceptingInput = false;

        SetupCards();
        StartPreview();
    }

    public void ShowHint()
    {
        if (options == null) return;
        playerSequence.Clear();
        pickedCards.Clear();
        StartPreview();
    }

    public RectTransform GetCorrectButton()
    {
        if (targetSequence == null || targetSequence.Length == 0 || cards == null) return null;
        int index = options != null ? options.IndexOf(targetSequence[0]) : -1;
        if (index < 0 || index >= cards.Length || cards[index] == null) return null;
        return cards[index].GetComponent<RectTransform>();
    }

    private Button[] CollectCards()
    {
        List<Button> result = new List<Button>();
        Button[] assignedCards = { card1, card2, card3, card4, card5, card6, card7, card8 };

        foreach (Button button in assignedCards)
            AddCard(result, button);

        Button[] childButtons = GetComponentsInChildren<Button>(true);
        Array.Sort(childButtons, CompareQuestionButtons);

        foreach (Button button in childButtons)
        {
            if (button.gameObject.name.StartsWith("Question", StringComparison.OrdinalIgnoreCase))
                AddCard(result, button);
        }

        if (result.Count == 0)
        {
            foreach (Button button in childButtons)
                AddCard(result, button);
        }

        return result.ToArray();
    }

    private void AddCard(List<Button> result, Button button)
    {
        if (button != null && !result.Contains(button))
            result.Add(button);
    }

    private int CompareQuestionButtons(Button a, Button b)
    {
        return GetQuestionNumber(a).CompareTo(GetQuestionNumber(b));
    }

    private int GetQuestionNumber(Button button)
    {
        string name = button != null ? button.gameObject.name : "";
        string digits = "";
        foreach (char c in name)
        {
            if (char.IsDigit(c))
                digits += c;
        }

        return int.TryParse(digits, out int value) ? value : int.MaxValue;
    }

    private void SetupCards()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            Button card = cards[i];
            if (card == null) continue;

            bool hasOption = options != null && i < options.Count;
            card.gameObject.SetActive(hasOption);
            card.interactable = false;

            if (!hasOption) continue;

            string value = options[i];
            SetCardFace(card, value, false, hiddenColor);

            card.onClick.RemoveAllListeners();
            Button capturedCard = card;
            string capturedValue = value;
            card.onClick.AddListener(() => OnCardClicked(capturedCard, capturedValue));
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
        SetCardsInteractable(false);
        HideAllCards();

        yield return new WaitForSeconds(previewDelay);

        foreach (string value in targetSequence)
        {
            int index = options != null ? options.IndexOf(value) : -1;
            if (index >= 0 && index < cards.Length && cards[index] != null)
            {
                SetCardFace(cards[index], value, true, previewColor);
                yield return new WaitForSeconds(previewHold);
                SetCardFace(cards[index], value, false, hiddenColor);
                yield return new WaitForSeconds(previewDelay);
            }
        }

        acceptingInput = true;
        SetCardsInteractable(true);
    }

    private void OnCardClicked(Button card, string value)
    {
        if (!acceptingInput) return;
        if (playerSequence.Count >= targetSequence.Length) return;
        if (pickedCards.Contains(card)) return;

        int index = playerSequence.Count;
        bool isCorrectPick = index < targetSequence.Length && value == targetSequence[index];

        playerSequence.Add(value);
        pickedCards.Add(card);
        SetCardFace(card, value, true, isCorrectPick ? pickedColor : wrongColor);
        card.interactable = false;

        if (!isCorrectPick)
        {
            acceptingInput = false;
            SetCardsInteractable(false);
            StartCoroutine(SubmitAfterDelay(string.Join("|", playerSequence)));
            return;
        }

        if (playerSequence.Count >= targetSequence.Length)
        {
            acceptingInput = false;
            SetCardsInteractable(false);
            StartCoroutine(SubmitAfterDelay(string.Join("|", playerSequence)));
        }
    }

    private IEnumerator SubmitAfterDelay(string answer)
    {
        yield return new WaitForSeconds(submitDelay);
        onAnswerSelected?.Invoke(answer);
    }

    private void HideAllCards()
    {
        if (cards == null) return;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null && cards[i].gameObject.activeSelf)
            {
                string value = options != null && i < options.Count ? options[i] : "";
                SetCardFace(cards[i], value, false, hiddenColor);
            }
        }
    }

    private void SetCardsInteractable(bool interactable)
    {
        if (cards == null) return;
        foreach (Button card in cards)
        {
            if (card != null && card.gameObject.activeSelf && !pickedCards.Contains(card))
                card.interactable = interactable;
        }
    }

    private void SetCardFace(Button card, string value, bool revealed, Color color)
    {
        SetCardColor(card, color);

        TextMeshProUGUI label = card.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null) label.gameObject.SetActive(false);

        Image contentImage = FindContentImage(card);
        if (contentImage == null) return;

        contentImage.gameObject.SetActive(revealed);
        if (!revealed) return;

        Sprite sprite = Resources.Load<Sprite>(value);
        contentImage.sprite = sprite;
        contentImage.preserveAspect = true;
        contentImage.color = Color.white;

        if (sprite == null)
            Debug.LogWarning($"[WorkingMemory] Gambar gagal load: {value}");
    }

    private void SetCardColor(Button card, Color color)
    {
        Image image = card.GetComponent<Image>();
        if (image != null)
            image.color = color;
    }

    private Image FindContentImage(Button card)
    {
        Image[] images = card.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (image.gameObject != card.gameObject)
                return image;
        }
        return null;
    }
}
