using System.Collections;
using UnityEngine;

/// <summary>
/// AudioManager — singleton DontDestroyOnLoad.
/// 
/// Setup di Unity:
///   1. Buat empty GameObject "AudioManager" di scene pertama (HomeScreen).
///   2. Attach script ini.
///   3. Assign di Inspector:
///        - bgmHome     : Joyfull_lullaby
///        - bgmGameplay : Dancing_silly
///   4. Pastikan kedua AudioSource "Loop" = true dan "Play On Awake" = false.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    public AudioClip bgmHome;       // Joyfull_lullaby
    public AudioClip bgmGameplay;   // Dancing_silly

    [Header("Settings")]
    [Range(0f, 1f)] public float volumeHome     = 0.3f;
    [Range(0f, 1f)] public float volumeGameplay = 0.6f;
    public float fadeDuration = 1.0f;

    private AudioSource _source;
    private Coroutine   _fadeCoroutine;

    // ── Lifecycle ─────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = gameObject.AddComponent<AudioSource>();
        _source.loop        = true;
        _source.playOnAwake = false;

        PlayHome(); // mulai dengan BGM homescreen
    }

    // ── Public API ────────────────────────────────────────────────

    /// <summary>Panggil saat masuk HomeScreen / MapScreen.</summary>
    public void PlayHome()
    {
        if (bgmHome == null) return;
        SwitchTo(bgmHome, volumeHome);
    }

    /// <summary>Panggil saat GameSession dimulai.</summary>
    public void PlayGameplay()
    {
        if (bgmGameplay == null) return;
        SwitchTo(bgmGameplay, volumeGameplay);
    }

    public void StopAll()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _source.Stop();
    }

    // ── Internal ──────────────────────────────────────────────────

    private void SwitchTo(AudioClip clip, float targetVolume)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        if (_source.clip == clip && _source.isPlaying) return; // sudah main

        _fadeCoroutine = StartCoroutine(FadeSwitch(clip, targetVolume));
    }

    private IEnumerator FadeSwitch(AudioClip clip, float targetVolume)
    {
        // Fade out kalau ada yang lagi main
        if (_source.isPlaying)
        {
            float startVol = _source.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                _source.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
                yield return null;
            }
            _source.Stop();
            _source.volume = 0f;
        }

        // Ganti clip & fade in
        _source.clip = clip;
        _source.Play();
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            _source.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
            yield return null;
        }
        _source.volume = targetVolume;
    }
}
