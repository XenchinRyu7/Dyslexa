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

    // AdaptiveMode secara permanen ML (1), properties dihilangkan untuk UI,
    // tetapi kita masih menyediakan properti ini jika dibutuhkan backward compat
    public static bool UseML => true;

    void Start()
    {
        if (settingsWindow != null)
            settingsWindow.SetActive(false);

        // Button listeners
        if (btnSettings != null) btnSettings.onClick.AddListener(OpenSettingsWindow);
        if (btnExcel    != null) btnExcel.onClick.AddListener(OnExportExcelClicked);
        if (btnPdf      != null) btnPdf.onClick.AddListener(OnExportPdfClicked);
        if (btnClose    != null) btnClose.onClick.AddListener(CloseSettingsWindow);

        // Toggle adaptive mode dihilangkan, sistem selalu pure ML
    }

    // ── Window Visibility ─────────────────────────────────────────

    public void OpenSettingsWindow()
    {
        if (settingsWindow != null)
        {

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
