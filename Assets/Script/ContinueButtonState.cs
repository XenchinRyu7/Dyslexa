using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pasang script ini langsung ke Tombol "Continue" / "Lanjutkan" di scene HomeScreen.
/// Script ini otomatis mengecek apakah ada profil saat layar pertama kali dimuat.
/// Jika profil kosong (0), tombol otomatis berubah warna jadi #333333.
/// </summary>
public class ContinueButtonState : MonoBehaviour
{
    void Start()
    {
        CheckProfileState();
    }

    private void CheckProfileState()
    {
        // Cek jika belum ada profil sama sekali
        if (PlayerProfileManager.Instance != null && PlayerProfileManager.Instance.GetAllProfiles().Count == 0)
        {
            // Ambil komponen Image dari tombol ini
            Image btnImage = GetComponent<Image>();
            if (btnImage != null)
            {
                Color greyColor;
                if (ColorUtility.TryParseHtmlString("#333333", out greyColor))
                {
                    btnImage.color = greyColor;
                }
            }
        }
    }
}
