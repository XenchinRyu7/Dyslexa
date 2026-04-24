using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Panel Fonologis Segmenting.
///
/// Prefab structure:
///   PanelSegmenting  [root — FonologisSegmentingPanel di sini]
///   ├── BtnLayout    [HorizontalLayoutGroup]
///   │   ├── BtnSound1   [draggable tile]
///   │   │   ├── Text    [TMP — menampilkan suku kata, e.g. "BO"]
///   │   │   └── Answer  [Button — tap untuk play audio suku kata]
///   │   ├── BtnSound2
///   │   └── BtnSound3
///   ├── Question1
///   │   └── Image       [gambar stimulus]
///   └── Layout Answer
///       ├── AnswerContainer1  [dock slot 1]
///       └── AnswerContainer2  [dock slot 2]
///
/// AudioSource: Add Component di root PanelSegmenting, uncheck Play On Awake.
/// Script auto-find AudioSource via GetComponent.
/// </summary>
public class FonologisSegmentingPanel : MonoBehaviour, IHintable
{
    [Header("Stimulus")]
    public Image stimulusImage;         // Question1/Image

    [Header("Syllable Bank (static, max 3)")]
    public GameObject syllable1;        // BtnLayout/BtnSound1
    public GameObject syllable2;        // BtnLayout/BtnSound2
    public GameObject syllable3;        // BtnLayout/BtnSound3

    [Header("Answer Slots (static, 2 dock)")]
    public GameObject slot1;            // AnswerContainer1
    public GameObject slot2;            // AnswerContainer2

    [Header("Submit")]
    public UnityEngine.UI.Button submitButton;

    [Header("Reset")]
    public UnityEngine.UI.Button resetButton; // Tombol Ulang — kembalikan semua tile ke bank

    // AudioSource di-find otomatis dari root GO di Awake
    private AudioSource audioSource;

    private Action<string>     onAnswerSelected;
    private Question           currentQuestion;
    private GameObject[]       syllableObjects;
    private SyllableDropSlot[] dropSlots;

    void Awake()
    {
        // Cari AudioSource di root ATAU child GO
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void ShowQuestion(Question question, Action<string> callback)
    {
        onAnswerSelected = callback;
        currentQuestion  = question;

        // Disable submit button sampai semua slot terisi
        if (submitButton != null)
        {
            submitButton.interactable = false;
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmit);
        }

        // Reset button — kembalikan semua tile ke bank
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetAllSlots);
        }

        // ── Gambar Stimulus ──────────────────────────────
        if (stimulusImage != null && !string.IsNullOrEmpty(question.stimulusImagePath))
        {
            Sprite sp = Resources.Load<Sprite>(question.stimulusImagePath);
            if (sp != null) stimulusImage.sprite = sp;
            else Debug.LogWarning($"[Segmenting] Gambar tidak ditemukan: {question.stimulusImagePath}");
        }

        // ── Setup 3 BtnSound ─────────────────────────────
        syllableObjects = new GameObject[] { syllable1, syllable2, syllable3 };
        string[] syllables = question.allSyllables;  // sudah di-shuffle oleh generator

        for (int i = 0; i < syllableObjects.Length; i++)
        {
            GameObject obj = syllableObjects[i];
            if (obj == null) continue;

            if (i < syllables.Length)
            {
                obj.SetActive(true);

                // Set teks suku kata
                TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = syllables[i];

                // Setup DraggableSyllable untuk drag ke slot
                // Auto-add komponen yang dibutuhkan
                if (obj.GetComponent<CanvasGroup>() == null)
                    obj.AddComponent<CanvasGroup>();

                DraggableSyllable drag = obj.GetComponent<DraggableSyllable>();
                if (drag == null) drag = obj.AddComponent<DraggableSyllable>();

                string audioPath = (question.syllableAudios != null && i < question.syllableAudios.Length)
                    ? question.syllableAudios[i] : "";

                drag.Setup(syllables[i], audioPath, audioSource, this, i);

                // Hubungkan tombol "Answer" (speaker) ke play audio
                ConnectAnswerButton(obj, audioPath);
            }
            else
            {
                obj.SetActive(false);
            }
        }

        // ── Setup 2 Dock Slot ────────────────────────────
        GameObject[] slotObjects = { slot1, slot2 };
        dropSlots = new SyllableDropSlot[slotObjects.Length];

        int activeSlots = question.correctSyllables?.Length ?? 2;

        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotObjects[i] == null) continue;

            slotObjects[i].SetActive(i < activeSlots);

            SyllableDropSlot ds = slotObjects[i].GetComponent<SyllableDropSlot>();
            if (ds == null) ds = slotObjects[i].AddComponent<SyllableDropSlot>();

            ds.slotIndex = i;
            ds.panel     = this;
            ds.Clear();
            dropSlots[i] = ds;
        }
    }

    // ── AUDIO ────────────────────────────────────────────

    /// <summary>
    /// Cari child "Answer" Button di dalam BtnSound dan assign onClick play audio.
    /// </summary>
    private void ConnectAnswerButton(GameObject btnSound, string audioPath)
    {
        // Cari child bernama "Answer"
        Transform answerTransform = btnSound.transform.Find("Answer");
        if (answerTransform == null)
        {
            // Fallback: cari Button selain root
            Button[] all = btnSound.GetComponentsInChildren<Button>(true);
            foreach (Button b in all)
            {
                if (b.gameObject != btnSound)
                { answerTransform = b.transform; break; }
            }
        }

        if (answerTransform == null) return;

        Button answerBtn = answerTransform.GetComponent<Button>();
        if (answerBtn == null) return;

        answerBtn.onClick.RemoveAllListeners();
        string capturedPath = audioPath;
        answerBtn.onClick.AddListener(() => PlaySyllableAudio(capturedPath));
    }

    public void PlaySyllableAudio(string audioPath)
    {
        if (string.IsNullOrEmpty(audioPath) || audioSource == null) return;
        AudioClip clip = Resources.Load<AudioClip>(audioPath);
        if (clip != null) audioSource.PlayOneShot(clip);
        else Debug.LogWarning($"[Segmenting] Audio tidak ditemukan: {audioPath}");
    }

    // ── CALLBACK DARI SyllableDropSlot ──────────────────

    public void OnSlotFilled(int slotIndex, string syllable)
    {
        CheckIfComplete();
    }

    public void OnSlotCleared(int slotIndex) { CheckIfComplete(); }

    // ── RESET: kembalikan semua tile ke bank ─────────────

    public void ResetAllSlots()
    {
        if (dropSlots != null)
            foreach (SyllableDropSlot slot in dropSlots)
                slot?.Clear();

        if (submitButton != null)
            submitButton.interactable = false;
    }

    // ── HINT: glow BtnSound pertama yang benar ────────────
    public void ShowHint()
    {
        if (currentQuestion?.correctSyllables == null || syllableObjects == null) return;

        string firstCorrect = currentQuestion.correctSyllables[0];

        foreach (GameObject obj in syllableObjects)
        {
            if (obj == null || !obj.activeSelf) continue;
            TextMeshProUGUI lbl = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null && lbl.text == firstCorrect)
            {
                StartCoroutine(GlowBtnSound(obj));
                return;
            }
        }
    }

    private System.Collections.IEnumerator GlowBtnSound(GameObject btn)
    {
        // Cari TMP text di BtnSound (lebih reliable dari Image karena bisa SVG)
        TMPro.TextMeshProUGUI lbl = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (lbl == null)
        {
            Debug.LogWarning("[Hint-Seg] TMP text tidak ditemukan di BtnSound!");
            yield break;
        }

        Color original = lbl.color;
        Color green    = new Color(0.1f, 0.85f, 0.2f, 1f);

        // Kedip hijau 4x
        for (int i = 0; i < 4; i++)
        {
            lbl.color = green;
            yield return new WaitForSeconds(0.3f);
            lbl.color = original;
            yield return new WaitForSeconds(0.2f);
        }
        // Tahan hijau 1 detik biar anak sempat lihat
        lbl.color = green;
        yield return new WaitForSeconds(1.0f);
        lbl.color = original;
    }

    // ── SUBMIT ───────────────────────────────────────────

    /// <summary>
    /// Dipanggil saat user klik tombol Submit/Cek.
    /// </summary>
    private void OnSubmit()
    {
        if (dropSlots == null) return;

        System.Collections.Generic.List<string> answers =
            new System.Collections.Generic.List<string>();

        foreach (SyllableDropSlot slot in dropSlots)
        {
            if (slot != null && slot.gameObject.activeSelf)
                answers.Add(slot.GetSyllableText() ?? "");
        }

        string result = string.Join("-", answers);
        Debug.Log($"[Segmenting] Submit: {result} | Benar: {currentQuestion?.correctAnswer}");
        onAnswerSelected?.Invoke(result);
    }

    // ── AUTO-CHECK ───────────────────────────────────────

    private void CheckIfComplete()
    {
        if (dropSlots == null) return;

        bool allFilled = true;
        foreach (SyllableDropSlot slot in dropSlots)
        {
            if (slot == null || !slot.gameObject.activeSelf) continue;
            if (!slot.IsOccupied) { allFilled = false; break; }
        }

        // Enable submit button ketika semua slot terisi
        if (submitButton != null)
            submitButton.interactable = allFilled;
    }
}
