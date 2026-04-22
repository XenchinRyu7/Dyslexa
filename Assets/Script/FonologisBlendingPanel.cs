using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Panel Fonologis Blending.
/// Prefab: PanelBlending > TitleInput, Layout > Question1-4 (Button > Image)
/// </summary>
public class FonologisBlendingPanel : MonoBehaviour
{
    [Header("Stimulus")]
    public Button playSoundButton;

    [Header("4 Image Buttons (fixed)")]
    public Button question1;
    public Button question2;
    public Button question3;
    public Button question4;

    [Header("Audio")]
    public AudioSource audioSource;

    private Action<string> onAnswerSelected;
    private AudioClip currentClip;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (playSoundButton != null)
            playSoundButton.onClick.AddListener(PlayAudio);
    }

    public void ShowQuestion(Question question, Action<string> callback)
    {
        onAnswerSelected = callback;

        currentClip = Resources.Load<AudioClip>(question.audioClipName);
        PlayAudio();

        Button[] buttons = { question1, question2, question3, question4 };
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null || i >= question.imageOptions.Count) continue;

            Image img = buttons[i].GetComponentInChildren<Image>();
            if (img != null)
            {
                Sprite sp = Resources.Load<Sprite>(question.imageOptions[i]);
                if (sp != null) img.sprite = sp;
                else Debug.LogWarning($"[Blending] Gambar tidak ditemukan: {question.imageOptions[i]}");
            }

            buttons[i].onClick.RemoveAllListeners();
            int idx = i;
            buttons[i].onClick.AddListener(() => onAnswerSelected?.Invoke(question.imageOptions[idx]));
        }
    }

    public void PlayAudio()
    {
        if (currentClip != null && audioSource != null)
            audioSource.PlayOneShot(currentClip);
    }
}
