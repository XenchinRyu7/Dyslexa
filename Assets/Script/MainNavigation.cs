using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainNavigation : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject exitConfirmationWindow;

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("HomeScreen");
    }

    public void LoadNewGame()
    {
        SceneManager.LoadScene("NewGame");
    }

    public void loadOnboardingAge()
    {
        SceneManager.LoadScene("OnboardingAge");
    }

    public void LoadContinue()
    {
        // Cek jika belum ada profil sama sekali
        if (PlayerProfileManager.Instance != null && PlayerProfileManager.Instance.GetAllProfiles().Count == 0)
        {
            // Tampilkan popup android
            PlayerProfileManager.Instance.ShowAndroidToast("Silakan buat profil baru terlebih dahulu!");
            
            // Ubah warna tombol jadi abu-abu hex #333333
            GameObject clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (clickedButton != null)
            {
                UnityEngine.UI.Image btnImage = clickedButton.GetComponent<UnityEngine.UI.Image>();
                if (btnImage != null)
                {
                    Color greyColor;
                    if (ColorUtility.TryParseHtmlString("#333333", out greyColor))
                    {
                        btnImage.color = greyColor;
                    }
                }
            }
            Debug.LogWarning("[MainNavigation] Akses ditolak: Tidak ada profil tersimpan.");
            return; // Batalkan pindah scene
        }

        SceneManager.LoadScene("ContinueGame");
    }

    public void LoadChooseMode()
    {
        SceneManager.LoadScene("ChooseMode");
    }

    public void LoadLevelMap()
    {
        SceneManager.LoadScene("LevelMap");
    }

    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    public void LoadPreviousScene()
    {
        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (previousSceneIndex >= 0)
        {
            SceneManager.LoadScene(previousSceneIndex);
        }
    }
    // --- ONBOARDING DATA SUBMISSION ---
    [Header("Onboarding Inputs")]
    public TMP_InputField nameInputField;

    // Dipanggil oleh tombol "Lanjut" di scene NewGame
    // Fungsi ini tidak butuh parameter, dia langsung baca isi nameInputField
    public void SubmitNameFromInput()
    {
        if (nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text))
        {
            string playerName = nameInputField.text.Trim();

            if (PlayerProfileManager.Instance.IsProfileNameTaken(playerName))
            {
                string message = "Nama sudah ada, gunakan nama lain.";
                PlayerProfileManager.Instance.ShowAndroidToast(message);
                Debug.LogWarning($"[MainNavigation] Nama profil duplikat: {playerName}");
                return;
            }

            PlayerProfileManager.Instance.SetTempName(playerName);
            SceneManager.LoadScene("OnboardingAge");
        }
        else
        {
            string message = "Nama kosong, isi dulu ya.";
            PlayerProfileManager.Instance.ShowAndroidToast(message);
            Debug.LogWarning("[MainNavigation] Nama kosong, isi input field terlebih dahulu!");
        }
    }

    // Dipanggil oleh tombol "Umur" tanpa parameter. Dia akan otomatis membaca teks di dalam tombol yang diklik.
    public void SubmitAgeFromButton()
    {
        GameObject clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (clickedButton != null)
        {
            TextMeshProUGUI buttonText = clickedButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null && int.TryParse(buttonText.text, out int age))
            {
                PlayerProfileManager.Instance.SetTempAge(age);
                SceneManager.LoadScene("OnboardingGender");
            }
            else
            {
                Debug.LogWarning("Teks di tombol tidak berupa angka, gagal menyimpan umur.");
            }
        }
    }

    // Dipanggil oleh tombol "Gender" tanpa parameter. Membaca otomatis dari teks ("Laki-Laki" / "Perempuan").
    public void SubmitGenderFromButton()
    {
        GameObject clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (clickedButton != null)
        {
            TextMeshProUGUI buttonText = clickedButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                PlayerProfileManager.Instance.SetTempGenderAndSave(buttonText.text);
                SceneManager.LoadScene("ChooseMode"); 
            }
        }
    }

    // --- PEMILIHAN MODE (CHOOSE MODE) ---
    public void SelectModeFonologis()
    {
        PlayerPrefs.SetString("SelectedGameMode", "Fonologis");
        PlayerPrefs.Save();
        SceneManager.LoadScene("LevelMap");
    }

    public void SelectModeVisual()
    {
        PlayerPrefs.SetString("SelectedGameMode", "Visual");
        PlayerPrefs.Save();
        SceneManager.LoadScene("LevelMap");
    }

    public void SelectModeWorkingMemory()
    {
        PlayerPrefs.SetString("SelectedGameMode", "WorkingMemory");
        PlayerPrefs.Save();
        SceneManager.LoadScene("LevelMap");
    }

    // --- EXIT CONFIRMATION ---
    public void ShowExitConfirmation()
    {
        if (exitConfirmationWindow != null)
        {
            exitConfirmationWindow.SetActive(true);
        }
    }

    public void HideExitConfirmation()
    {
        if (exitConfirmationWindow != null)
        {
            exitConfirmationWindow.SetActive(false);
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
