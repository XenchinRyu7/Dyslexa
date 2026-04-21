using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ChooseModeManager : MonoBehaviour
{
    [Header("UI Profile Info")]
    public TextMeshProUGUI greetingText;   // Contoh: "Halo, Budi!"

    void Start()
    {
        DisplayActiveProfile();
    }

    private void DisplayActiveProfile()
    {
        PlayerProfile activeProfile = PlayerProfileManager.Instance?.ActiveProfile;

        if (activeProfile == null)
        {
            Debug.LogWarning("[ChooseModeManager] Tidak ada ActiveProfile! Kembali ke ContinueGame.");
            // Jika entah bagaimana profil tidak ada, kembali ke layar pilih akun
            SceneManager.LoadScene("ContinueGame");
            return;
        }

        Debug.Log($"[ChooseModeManager] Active profile loaded: {activeProfile.playerName}");

        if (greetingText != null)
            greetingText.text = $"Halo, {activeProfile.playerName}!";
    }

    // --- PEMILIHAN MODE ---
    // Dipanggil oleh tombol Fonologis
    public void SelectFonologis()
    {
        PlayerPrefs.SetString("SelectedGameMode", "Fonologis");
        PlayerPrefs.Save();
        Debug.Log("[ChooseModeManager] Mode dipilih: Fonologis");
        SceneManager.LoadScene("LevelMap");
    }

    // Dipanggil oleh tombol Visual
    public void SelectVisual()
    {
        PlayerPrefs.SetString("SelectedGameMode", "Visual");
        PlayerPrefs.Save();
        Debug.Log("[ChooseModeManager] Mode dipilih: Visual");
        SceneManager.LoadScene("LevelMap");
    }

    // Dipanggil oleh tombol Mixed (opsional)
    public void SelectMixed()
    {
        PlayerPrefs.SetString("SelectedGameMode", "Mixed");
        PlayerPrefs.Save();
        Debug.Log("[ChooseModeManager] Mode dipilih: Mixed");
        SceneManager.LoadScene("LevelMap");
    }

    // Tombol Kembali ke layar pilih akun
    public void GoBack()
    {
        SceneManager.LoadScene("ContinueGame");
    }
}
