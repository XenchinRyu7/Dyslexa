using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Attach ke setiap slot (AnswerContainer1/2).
/// Terima drop dari DraggableSyllable.
/// Tap slot terisi → suku kata balik ke bank.
/// </summary>
public class SyllableDropSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [HideInInspector] public int slotIndex;
    [HideInInspector] public FonologisSegmentingPanel panel;

    private DraggableSyllable occupant;
    private TextMeshProUGUI   slotLabel;
    private Image             bg;

    // Warna slot kosong vs terisi
    private static readonly Color colorEmpty  = new Color(1f, 1f, 1f, 0.3f);
    private static readonly Color colorFilled = new Color(0.3f, 0.8f, 0.3f, 0.8f);

    void Awake()
    {
        slotLabel = GetComponentInChildren<TextMeshProUGUI>();
        bg        = GetComponent<Image>();
        SetVisualEmpty();
    }

    public bool   IsOccupied      => occupant != null;
    public string GetSyllableText() => occupant?.syllableText;

    // ── DROP ────────────────────────────────────────

    public void OnDrop(PointerEventData e)
    {
        DraggableSyllable dragged = e.pointerDrag?.GetComponent<DraggableSyllable>();
        if (dragged == null) return;

        // Kalau slot sudah terisi, kembalikan occupant dulu ke bank
        if (occupant != null)
        {
            occupant.ReturnToBank();
            occupant = null;
        }

        // Tempatkan tile di slot
        PlaceSyllable(dragged);
    }

    // ── TAP SLOT TERISI → kembali ke bank ──────────

    public void OnPointerClick(PointerEventData e)
    {
        if (occupant == null) return;

        DraggableSyllable toReturn = occupant;
        occupant = null;
        SetVisualEmpty();
        toReturn.ReturnToBank();
        panel?.OnSlotCleared(slotIndex);
    }

    // ── INTERNAL ─────────────────────────────────────

    private void PlaceSyllable(DraggableSyllable tile)
    {
        occupant = tile;

        // Pindahkan tile ke dalam slot secara visual
        RectTransform tileRt = tile.GetComponent<RectTransform>();
        tile.transform.SetParent(transform, false);
        tileRt.anchoredPosition   = Vector2.zero;
        tileRt.anchorMin          = Vector2.zero;
        tileRt.anchorMax          = Vector2.one;
        tileRt.offsetMin          = Vector2.zero;
        tileRt.offsetMax          = Vector2.zero;
        tile.GetComponent<CanvasGroup>().blocksRaycasts = true;
        tile.GetComponent<CanvasGroup>().alpha          = 1f;

        SetVisualFilled(tile.syllableText);
        panel?.OnSlotFilled(slotIndex, tile.syllableText);
    }

    private void SetVisualEmpty()
    {
        if (bg        != null) bg.color        = colorEmpty;
        if (slotLabel != null) slotLabel.text  = "";
    }

    private void SetVisualFilled(string text)
    {
        if (bg        != null) bg.color        = colorFilled;
        if (slotLabel != null) slotLabel.text  = text;
    }

    public void Clear()
    {
        occupant = null;
        SetVisualEmpty();
    }
}
