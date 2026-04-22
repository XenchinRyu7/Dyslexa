using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Panel Fonologis Segmenting — static children, drag-to-place.
///
/// Prefab structure:
///   PanelSegmenting
///   ├── Question1 > Image        (stimulus gambar)
///   ├── BtnLayout                (HorizontalLayoutGroup)
///   │   ├── BtnSound1            (suku kata 1)
///   │   ├── BtnSound2            (suku kata 2)
///   │   └── BtnSound3            (suku kata 3 / distraktor)
///   ├── AnswerContainer1         (slot jawaban 1)
///   └── AnswerContainer2         (slot jawaban 2)
///
/// AudioSource: taruh di root PanelSegmenting, assign di Inspector.
/// </summary>
public class FonologisSegmentingPanel : MonoBehaviour
{
    [Header("Stimulus")]
    public Image stimulusImage;

    [Header("Syllable Bank (max 3 BtnSound, static)")]
    public GameObject syllable1;
    public GameObject syllable2;
    public GameObject syllable3;

    [Header("Answer Slots (static)")]
    public GameObject slot1;
    public GameObject slot2;

    [Header("Audio")]
    public AudioSource audioSource;

    private Action<string> onAnswerSelected;
    private Question       currentQuestion;

    private GameObject[]  syllableObjects;
    private SyllableDropSlot[] dropSlots;
    private string[]      slotAnswers;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void ShowQuestion(Question question, Action<string> callback)
    {
        onAnswerSelected = callback;
        currentQuestion  = question;

        // Load gambar stimulus
        if (stimulusImage != null && !string.IsNullOrEmpty(question.stimulusImagePath))
        {
            Sprite sp = Resources.Load<Sprite>(question.stimulusImagePath);
            if (sp != null) stimulusImage.sprite = sp;
            else Debug.LogWarning($"[Segmenting] Gambar tidak ditemukan: {question.stimulusImagePath}");
        }

        // Setup syllable bank (max 3)
        syllableObjects = new GameObject[] { syllable1, syllable2, syllable3 };
        string[] syllables = question.allSyllables;

        for (int i = 0; i < syllableObjects.Length; i++)
        {
            if (syllableObjects[i] == null) continue;

            if (i < syllables.Length)
            {
                syllableObjects[i].SetActive(true);

                DraggableSyllable drag = syllableObjects[i].GetComponent<DraggableSyllable>();
                if (drag == null) drag = syllableObjects[i].AddComponent<DraggableSyllable>();

                string audio = (question.syllableAudios != null && i < question.syllableAudios.Length)
                    ? question.syllableAudios[i] : "";

                drag.Setup(syllables[i], audio, audioSource, this, i);
            }
            else
            {
                // Sembunyikan slot yang tidak terpakai
                syllableObjects[i].SetActive(false);
            }
        }

        // Setup answer slots (max 2)
        GameObject[] slotObjects = { slot1, slot2 };
        dropSlots   = new SyllableDropSlot[slotObjects.Length];
        slotAnswers = new string[slotObjects.Length];

        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotObjects[i] == null) continue;

            SyllableDropSlot ds = slotObjects[i].GetComponent<SyllableDropSlot>();
            if (ds == null) ds = slotObjects[i].AddComponent<SyllableDropSlot>();

            ds.slotIndex = i;
            ds.panel     = this;
            ds.Clear();
            dropSlots[i] = ds;
        }

        // Jumlah slot yang aktif = panjang jawaban benar
        int activeSlots = question.correctSyllables?.Length ?? 2;
        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotObjects[i] != null)
                slotObjects[i].SetActive(i < activeSlots);
        }
    }

    // ── DIPANGGIL DARI SyllableDropSlot ─────────────

    public void OnSlotFilled(int slotIndex, string syllable)
    {
        if (slotAnswers == null || slotIndex >= slotAnswers.Length) return;
        slotAnswers[slotIndex] = syllable;
        CheckIfComplete();
    }

    public void OnSlotCleared(int slotIndex)
    {
        if (slotAnswers == null || slotIndex >= slotAnswers.Length) return;
        slotAnswers[slotIndex] = null;
    }

    // ── AUTO-CHECK SAAT SEMUA SLOT TERISI ──────────

    private void CheckIfComplete()
    {
        if (dropSlots == null) return;

        foreach (SyllableDropSlot slot in dropSlots)
        {
            if (slot == null || !slot.gameObject.activeSelf) continue;
            if (!slot.IsOccupied) return; // ada slot yang belum terisi
        }

        // Semua slot terisi → susun jawaban dan cek
        System.Collections.Generic.List<string> answers = new System.Collections.Generic.List<string>();
        foreach (SyllableDropSlot slot in dropSlots)
        {
            if (slot != null && slot.gameObject.activeSelf)
                answers.Add(slot.GetSyllableText() ?? "");
        }

        string result = string.Join("-", answers);
        Debug.Log($"[Segmenting] Jawaban user: {result} | Benar: {currentQuestion.correctAnswer}");
        onAnswerSelected?.Invoke(result);
    }
}
