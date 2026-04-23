using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// Panel Fonologis Blending.
///
/// Prefab structure:
///   PanelBlending  [root — FonologisBlendingPanel di sini + AudioSource]
///   ├── BtnPlaySound   [Button — replay audio]
///   └── Layout
///       ├── Question1  [Button > Image — pilihan gambar]
///       ├── Question2
///       ├── Question3
///       └── Question4
///
/// Gameplay: saat soal tampil, audio suku kata diputar satu per satu dengan jeda
/// berdasarkan difficulty. Anak mendengar lalu pilih gambar yang sesuai.
///
/// Delay antar suku kata per difficulty:
///   1 = 1000ms  (paling lambat, paling mudah)
///   2 = 800ms
///   3 = 600ms
///   4 = 400ms
///   5 = 200ms   (paling cepat)
/// </summary>
public class FonologisBlendingPanel : MonoBehaviour
{
    [Header("Tombol Replay")]
    public Button playSoundButton;

    [Header("4 Image Buttons (fixed)")]
    public Button question1;
    public Button question2;
    public Button question3;
    public Button question4;

    // AudioSource di-find otomatis (root atau child GO)
    private AudioSource   audioSource;
    private Action<string> onAnswerSelected;
    private Question      currentQuestion;
    private Coroutine     playRoutine;

    void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (playSoundButton != null)
            playSoundButton.onClick.AddListener(ReplayAudio);
    }

    public void ShowQuestion(Question question, Action<string> callback)
    {
        onAnswerSelected = callback;
        currentQuestion  = question;

        // Setup 4 image buttons
        Button[] buttons = { question1, question2, question3, question4 };
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null || i >= question.imageOptions.Count) continue;

            // Load gambar ke komponen Image dalam button
            Image img = buttons[i].GetComponentInChildren<Image>();
            if (img != null)
            {
                Sprite sp = Resources.Load<Sprite>(question.imageOptions[i]);
                if (sp != null) img.sprite = sp;
                else Debug.LogWarning($"[Blending] Gambar tidak ditemukan: {question.imageOptions[i]}");
            }

            // Klik button → kirim image path sebagai jawaban
            buttons[i].interactable = true;
            buttons[i].onClick.RemoveAllListeners();
            int   idx      = i;
            string imgPath = question.imageOptions[i];
            buttons[i].onClick.AddListener(() => onAnswerSelected?.Invoke(imgPath));
        }

        // Auto-play audio suku kata saat soal muncul
        PlaySyllablesSequentially();
    }

    // ── AUDIO ───────────────────────────────────────────

    public void ReplayAudio() => PlaySyllablesSequentially();

    private void PlaySyllablesSequentially()
    {
        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        if (currentQuestion?.syllableAudios == null) yield break;

        // Delay antar suku kata berdasarkan difficulty
        int   diff  = ProgressManager.Instance.GetCurrentDifficulty();
        float delay = Mathf.Lerp(1.0f, 0.2f, (diff - 1) / 4f); // 1=1000ms .. 5=200ms

        // Disable tombol gambar selama audio diputar
        SetButtonsInteractable(false);

        foreach (string audioPath in currentQuestion.syllableAudios)
        {
            if (string.IsNullOrEmpty(audioPath)) continue;

            AudioClip clip = Resources.Load<AudioClip>(audioPath);
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length + delay);
            }
            else
            {
                Debug.LogWarning($"[Blending] Audio tidak ditemukan: {audioPath}");
                yield return new WaitForSeconds(delay);
            }
        }

        // Enable tombol setelah audio selesai
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool state)
    {
        Button[] buttons = { question1, question2, question3, question4 };
        foreach (Button b in buttons)
            if (b != null) b.interactable = state;
    }
}
