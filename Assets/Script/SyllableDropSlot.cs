using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Attach ke AnswerContainer1/2 (dock slot).
/// Drop tile → slot tampilkan TEXT suku kata, tile di-hide dari bank.
/// Tap slot terisi → tile kembali muncul di bank, slot kembali kosong.
/// Butuh Image component (Raycast Target ON) agar OnDrop bisa fire.
/// </summary>
public class SyllableDropSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [HideInInspector] public int slotIndex;
    [HideInInspector] public FonologisSegmentingPanel panel;

    private DraggableSyllable occupant;
    private TextMeshProUGUI   slotLabel;

    void Awake()
    {
        // Cari TMP child untuk menampilkan teks suku kata
        slotLabel = GetComponentInChildren<TextMeshProUGUI>();
        SetVisualEmpty();
    }

    public bool   IsOccupied       => occupant != null;
    public string GetSyllableText() => occupant?.syllableText;

    // ── DROP → tampilkan teks, hide tile dari bank ──────

    public void OnDrop(PointerEventData e)
    {
        DraggableSyllable dragged = e.pointerDrag?.GetComponent<DraggableSyllable>();
        if (dragged == null) return;

        // Kalau slot sudah terisi, kembalikan tile lama ke bank dulu
        if (occupant != null)
        {
            occupant.ReturnToBank();
            occupant = null;
        }

        // Simpan referensi tile, tampilkan teks, sembunyikan tile dari bank
        occupant = dragged;
        dragged.HideFromBank();

        if (slotLabel != null) slotLabel.text = dragged.syllableText;
        SetVisualFilled();

        panel?.OnSlotFilled(slotIndex, dragged.syllableText);
    }

    // ── TAP SLOT TERISI → tile balik ke bank ────────────

    public void OnPointerClick(PointerEventData e)
    {
        if (occupant == null) return;

        DraggableSyllable toReturn = occupant;
        occupant = null;

        if (slotLabel != null) slotLabel.text = "";
        SetVisualEmpty();

        toReturn.ReturnToBank();
        panel?.OnSlotCleared(slotIndex);
    }

    // ── PUBLIC CLEAR (dipanggil saat soal baru) ──────────

    public void Clear()
    {
        if (occupant != null)
        {
            occupant.ReturnToBank();
            occupant = null;
        }
        if (slotLabel != null) slotLabel.text = "";
        SetVisualEmpty();
    }

    // ── VISUAL ──────────────────────────────────────────

    private void SetVisualEmpty()
    {
        Image bg = GetComponent<Image>();
        if (bg != null) bg.color = new Color(1f, 1f, 1f, 0.2f);
    }

    private void SetVisualFilled()
    {
        Image bg = GetComponent<Image>();
        if (bg != null) bg.color = new Color(0.3f, 0.85f, 0.4f, 0.85f);
    }
}
