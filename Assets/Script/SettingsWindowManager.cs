using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller untuk Settings Window di HomeScreen.
/// 
/// Setup di Unity Inspector:
///   - settingsWindow     : GameObject panel/window
///   - btnSettings        : Tombol buka settings
///   - btnExcel           : Export Excel
///   - btnPdf             : Export HTML/PDF
///   - btnClose           : Tutup window
///   - adaptiveModeToggle : Toggle — OFF=Rule Engine, ON=Machine Learning
///   - adaptiveModeLabel  : TextMeshPro label status (opsional)
/// </summary>
public class SettingsWindowManager : MonoBehaviour
{
    [Header("Window Panel")]
    public GameObject settingsWindow;

    [Header("Buttons")]
    public Button btnSettings;
    public Button btnExcel;
    public Button btnPdf;
    public Button btnClose;

    [Header("Adaptive Mode")]
    [Tooltip("Toggle: OFF = Rule Engine, ON = Machine Learning")]
    public Toggle adaptiveModeToggle;
    [Tooltip("Label status mode aktif (opsional)")]
    public TextMeshProUGUI adaptiveModeLabel;

    // PlayerPrefs key
    private const string KEY_ADAPTIVE_MODE = "AdaptiveMode";

    // 0 = Rule Engine, 1 = ML
    public static int  CurrentAdaptiveMode => PlayerPrefs.GetInt(KEY_ADAPTIVE_MODE, 0);
    public static bool UseML              => CurrentAdaptiveMode == 1;

    void Start()
    {
        if (settingsWindow != null)
            settingsWindow.SetActive(false);

        // Button listeners
        if (btnSettings != null) btnSettings.onClick.AddListener(OpenSettingsWindow);
        if (btnExcel    != null) btnExcel.onClick.AddListener(OnExportExcelClicked);
        if (btnPdf      != null) btnPdf.onClick.AddListener(OnExportPdfClicked);
        if (btnClose    != null) btnClose.onClick.AddListener(CloseSettingsWindow);

        // Toggle adaptive mode
        SetupAdaptiveToggle();
    }

    // ── Toggle Setup ───────────────────────────────────────────────

    private void SetupAdaptiveToggle()
    {
        if (adaptiveModeToggle == null) return;

        bool savedML = PlayerPrefs.GetInt(KEY_ADAPTIVE_MODE, 0) == 1;
        adaptiveModeToggle.isOn = savedML;
        UpdateModeLabel(savedML);
        adaptiveModeToggle.onValueChanged.AddListener(OnAdaptiveModeChanged);
    }

    private void OnAdaptiveModeChanged(bool isOn)
    {
        PlayerPrefs.SetInt(KEY_ADAPTIVE_MODE, isOn ? 1 : 0);
        PlayerPrefs.Save();
        UpdateModeLabel(isOn);
        Debug.Log($"[Settings] Adaptive mode: {(isOn ? "Machine Learning" : "Rule Engine")}");
    }

    private void UpdateModeLabel(bool isOn)
    {
        if (adaptiveModeLabel == null) return;
        adaptiveModeLabel.text = isOn
            ? "Mode Adaptif: <color=#10B981>Machine Learning</color>"
            : "Mode Adaptif: <color=#3B82F6>Rule Engine</color>";
    }

    // ── Window Visibility ─────────────────────────────────────────

    public void OpenSettingsWindow()
    {
        if (settingsWindow != null)
        {
            // Sync toggle ke nilai tersimpan setiap kali dibuka
            if (adaptiveModeToggle != null)
                adaptiveModeToggle.isOn = PlayerPrefs.GetInt(KEY_ADAPTIVE_MODE, 0) == 1;

            settingsWindow.SetActive(true);
            Debug.Log("[Settings] Window dibuka.");
        }
    }

    public void CloseSettingsWindow()
    {
        if (settingsWindow != null)
        {
            settingsWindow.SetActive(false);
            Debug.Log("[Settings] Window ditutup.");
        }
    }

    // ── Export Actions ────────────────────────────────────────────

    private void OnExportExcelClicked()
    {
        Debug.Log("[Settings] Export ke Excel...");
        if (DataExportManager.Instance != null)
            DataExportManager.Instance.ExportToCSV();
        else
        {
            PlayerProfileManager.Instance?.ExportToCSV();
            Debug.LogWarning("[Settings] DataExportManager tidak ditemukan, menggunakan fallback.");
        }
    }

    private void OnExportPdfClicked()
    {
        Debug.Log("[Settings] Export ke PDF/HTML Report...");
        if (DataExportManager.Instance != null)
            DataExportManager.Instance.ExportToPDF();
        else
            Debug.LogError("[Settings] DataExportManager tidak ditemukan!");
    }
}
