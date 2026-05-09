using UnityEngine;

/// <summary>
/// Pasang script ini di Container/Panel yang membungkus InputField.
/// Saat keyboard HP muncul, container akan otomatis naik secara halus (smooth).
/// </summary>
public class ShiftUIOnKeyboard : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    
    [Header("Pengaturan Keyboard")]
    [Tooltip("Seberapa tinggi panel akan naik saat keyboard muncul (sesuaikan dengan ukuran Canvas)")]
    public float shiftAmount = 400f; 
    
    [Tooltip("Kecepatan animasi naik/turun")]
    public float animationSpeed = 10f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // Cek apakah virtual keyboard HP sedang aktif
        if (TouchScreenKeyboard.visible)
        {
            // Naikkan UI secara halus menggunakan Lerp
            Vector2 targetPosition = new Vector2(originalPosition.x, originalPosition.y + shiftAmount);
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * animationSpeed);
        }
        else
        {
            // Turunkan kembali ke posisi awal saat keyboard ditutup
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, originalPosition, Time.deltaTime * animationSpeed);
        }
    }
}
