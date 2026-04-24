using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Attach ke setiap BtnSound di bank suku kata.
/// Drag ke slot. Tap (tanpa drag) = play audio.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DraggableSyllable : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [HideInInspector] public string syllableText;
    [HideInInspector] public string audioPath;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public FonologisSegmentingPanel panel;
    [HideInInspector] public int bankIndex;

    private Canvas        rootCanvas;
    private RectTransform rt;
    private CanvasGroup   cg;

    private Transform originalParent;
    private Vector2   originalPos;
    private int       originalSibling;

    private bool isDragging = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
    }

    public void Setup(string text, string audio, AudioSource source,
                      FonologisSegmentingPanel p, int index)
    {
        syllableText = text;
        audioPath    = audio;
        audioSource  = source;
        panel        = p;
        bankIndex    = index;

        TextMeshProUGUI tmp = GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;

        Canvas[] canvases = GetComponentsInParent<Canvas>(true);
        foreach (Canvas c in canvases)
            if (c.isRootCanvas) { rootCanvas = c; break; }
        if (rootCanvas == null && canvases.Length > 0)
            rootCanvas = canvases[canvases.Length - 1];
    }

    public void OnBeginDrag(PointerEventData e)
    {
        isDragging      = true;
        originalParent  = transform.parent;
        originalPos     = rt.anchoredPosition;
        originalSibling = transform.GetSiblingIndex();

        transform.SetParent(rootCanvas.transform, true);
        cg.alpha          = 0.75f;
        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        rt.anchoredPosition += e.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e)
    {
        isDragging = false;
        if (transform.parent == rootCanvas.transform)
            ReturnToBank();
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (!isDragging) PlayAudio();
    }

    public void ReturnToBank()
    {
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSibling);
        rt.anchoredPosition = originalPos;
        cg.alpha            = 1f;
        cg.blocksRaycasts   = true;
        gameObject.SetActive(true);
    }

    public void HideFromBank()
    {
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSibling);
        cg.alpha          = 1f;
        cg.blocksRaycasts = true;
        gameObject.SetActive(false);
    }

    public void PlayAudio()
    {
        if (string.IsNullOrEmpty(audioPath) || audioSource == null) return;
        AudioClip clip = Resources.Load<AudioClip>(audioPath);
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}
