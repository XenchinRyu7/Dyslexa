using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller untuk Settings Window di HomeScreen.
/// Attach ke GameObject manapun di scene HomeScreen.
/// 
/// Setup di Unity Inspector:
///   - settingsWindow  : GameObject panel/window yang akan di show/hide
///   - btnSettings     : Tombol gear/settings untuk membuka window
///   - btnExcel        : Tombol export ke CSV (Excel)
///   - btnPdf          : Tombol export ke HTML Report (PDF)
///   - btnClose        : Tombol X untuk menutup window
/// </summary>
public class SettingsWindowManager : MonoBehaviour
{
    [Header("Window Panel")]
    public GameObject settingsWindow; // Panel Settings yang di-show/hide

    [Header("Buttons")]
    public Button btnSettings; // Tombol di HomeScreen untuk buka window
    public Button btnExcel;    // Export CSV
    public Button btnPdf;      // Export HTML/PDF
    public Button btnClose;    // Tutup window

    void Start()
    {
        // Pastikan window tersembunyi saat start
        if (settingsWindow != null)
            settingsWindow.SetActive(false);

        // Assign listener ke semua tombol
        if (btnSettings != null)
            btnSettings.onClick.AddListener(OpenSettingsWindow);

        if (btnExcel != null)
            btnExcel.onClick.AddListener(OnExportExcelClicked);

        if (btnPdf != null)
            btnPdf.onClick.AddListener(OnExportPdfClicked);

        if (btnClose != null)
            btnClose.onClick.AddListener(CloseSettingsWindow);
    }

    // -------------------------------------------
    // WINDOW VISIBILITY
    // -------------------------------------------

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

    // -------------------------------------------
    // EXPORT ACTIONS
    // -------------------------------------------

    private void OnExportExcelClicked()
    {
        Debug.Log("[Settings] Export ke Excel/CSV...");

        if (DataExportManager.Instance != null)
        {
            DataExportManager.Instance.ExportToCSV();
        }
        else
        {
            // Fallback: gunakan method lama dari PlayerProfileManager
            PlayerProfileManager.Instance?.ExportToCSV();
            Debug.LogWarning("[Settings] DataExportManager tidak ditemukan, menggunakan fallback.");
        }
    }

    private void OnExportPdfClicked()
    {
        Debug.Log("[Settings] Export ke PDF/HTML Report...");

        if (DataExportManager.Instance != null)
        {
            DataExportManager.Instance.ExportToPDF();
        }
        else
        {
            Debug.LogError("[Settings] DataExportManager tidak ditemukan! Tambahkan ke scene.");
        }
    }
}
