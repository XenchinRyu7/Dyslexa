using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Attach ke AnswerContainer1/2 (dock slot).
/// Drop tile → slot tampilkan TEXT suku kata, tile di-hide dari bank.
/// Tap slot terisi → tile kembali muncul di bank, slot kembali kosong.
/// Image component harus ada dengan Raycast Target = ON.
/// </summary>
public class SyllableDropSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [HideInInspector] public int slotIndex;
    [HideInInspector] public FonologisSegmentingPanel panel;

    private DraggableSyllable occupant;
    private TextMeshProUGUI   slotLabel;

    void Awake()
    {
        slotLabel = GetComponentInChildren<TextMeshProUGUI>();
        SetVisualEmpty();
    }

    public bool   IsOccupied       => occupant != null;
    public string GetSyllableText() => occupant?.syllableText;

    public void OnDrop(PointerEventData e)
    {
        DraggableSyllable dragged = e.pointerDrag?.GetComponent<DraggableSyllable>();
        if (dragged == null) return;

        if (occupant != null)
        {
            occupant.ReturnToBank();
            occupant = null;
        }

        occupant = dragged;
        dragged.HideFromBank();

        if (slotLabel != null) slotLabel.text = dragged.syllableText;
        SetVisualFilled();

        panel?.OnSlotFilled(slotIndex, dragged.syllableText);
    }

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
