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
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            PlayerProfileManager.Instance.SetTempName(nameInputField.text);
            SceneManager.LoadScene("OnboardingAge");
        }
        else
        {
            Debug.LogWarning("Nama kosong, isi input field terlebih dahulu!");
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
