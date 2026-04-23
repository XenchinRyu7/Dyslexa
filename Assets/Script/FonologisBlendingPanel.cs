using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class FonologisBlendingPanel : MonoBehaviour
{
    [Header("Tombol Replay")]
    public Button playSoundButton;

    [Header("4 Image Buttons (fixed)")]
    public Button question1;
    public Button question2;
    public Button question3;
    public Button question4;

    private AudioSource    audioSource;
    private Action<string> onAnswerSelected;
    private Question       currentQuestion;
    private Coroutine      playRoutine;

    void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void ShowQuestion(Question question, Action<string> callback)
    {
        onAnswerSelected = callback;
        currentQuestion  = question;

        Debug.Log($"[Blending] ShowQuestion: {question.correctAnswer} | syllables: {question.syllableAudios?.Length} | imageOptions: {question.imageOptions?.Count}");

        // ── Reconnect play button + set text bubble ───────
        if (playSoundButton != null)
        {
            playSoundButton.onClick.RemoveAllListeners();
            playSoundButton.onClick.AddListener(ReplayAudio);

            // Set teks suku kata di bubble playSoundButton (e.g. "KA-ME-RA")
            // Derive dari nama file audio: "Audio/Blending/Kamera/ka" → "KA"
            if (question.syllableAudios != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (string path in question.syllableAudios)
                {
                    if (string.IsNullOrEmpty(path)) continue;
                    string[] parts = path.Split('/');
                    string sylText = parts[parts.Length - 1].ToUpper();
                    if (sb.Length > 0) sb.Append("-");
                    sb.Append(sylText);
                }
                TMPro.TextMeshProUGUI btnLabel =
                    playSoundButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (btnLabel != null) btnLabel.text = sb.ToString();
            }
        }
        else Debug.LogWarning("[Blending] playSoundButton TIDAK ASSIGNED di Inspector prefab!");

        // ── Setup 4 image buttons ─────────────────────────
        Button[] buttons = { question1, question2, question3, question4 };
        Debug.Log($"[Blending] imageOptions count = {question.imageOptions?.Count}");
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
            {   
                Debug.LogWarning($"[Blending] question{i+1} TIDAK ASSIGNED!");
                continue;
            }
            if (i >= question.imageOptions.Count) continue;

            string path = question.imageOptions[i];

            // Cari Image di semua children (ambil yang BUKAN milik button root)
            Image targetImg = null;
            Image[] allImgs = buttons[i].GetComponentsInChildren<Image>(true);
            Debug.Log($"[Blending] btn[{i}] '{buttons[i].name}' found {allImgs.Length} Image(s)");
            foreach (Image im in allImgs)
            {
                // Skip Image yang ada di GO yang sama dengan Button (background button)
                if (im.gameObject == buttons[i].gameObject) continue;
                targetImg = im;
                break;
            }
            // Fallback: pakai Image root kalau tidak ada child
            if (targetImg == null) targetImg = buttons[i].GetComponent<Image>();

            if (targetImg != null)
            {
                Sprite sp = Resources.Load<Sprite>(path);
                if (sp == null)
                {
                    Texture2D tex = Resources.Load<Texture2D>(path);
                    if (tex != null)
                        sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
                }
                if (sp != null)
                {
                    targetImg.sprite = sp;
                    Debug.Log($"[Blending] Gambar [{i}] loaded: {path}");
                }
                else
                    Debug.LogWarning($"[Blending] Gambar GAGAL load: {path}");
            }

            buttons[i].interactable = true;
            buttons[i].onClick.RemoveAllListeners();
            string imgPath = question.imageOptions[i];
            buttons[i].onClick.AddListener(() => onAnswerSelected?.Invoke(imgPath));
        }

        // Auto-play audio saat soal tampil
        PlaySyllablesSequentially();
    }

    // ── AUDIO ────────────────────────────────────────────

    public void ReplayAudio() => PlaySyllablesSequentially();

    private void PlaySyllablesSequentially()
    {
        if (playRoutine != null) StopCoroutine(playRoutine);
        audioSource.Stop(); // Hentikan audio lama sebelum mulai ulang
        playRoutine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        if (currentQuestion?.syllableAudios == null || currentQuestion.syllableAudios.Length == 0)
        {
            Debug.LogWarning("[Blending] syllableAudios null atau kosong!");
            yield break;
        }

        int   diff  = ProgressManager.Instance.GetCurrentDifficulty();
        float delay = Mathf.Lerp(1.0f, 0.2f, (diff - 1) / 4f);

        Debug.Log($"[Blending] Playing {currentQuestion.syllableAudios.Length} syllables, delay={delay:F2}s");

        foreach (string audioPath in currentQuestion.syllableAudios)
        {
            if (string.IsNullOrEmpty(audioPath)) continue;

            AudioClip clip = Resources.Load<AudioClip>(audioPath);
            if (clip != null)
            {
                Debug.Log($"[Blending] Playing: {audioPath}");
                audioSource.clip = clip;
                audioSource.Play();  // Play() bisa di-stop, PlayOneShot tidak
                yield return new WaitForSeconds(clip.length + delay);
            }
            else
            {
                Debug.LogWarning($"[Blending] Audio GAGAL load: {audioPath}");
                yield return new WaitForSeconds(delay);
            }
        }
    }
}
