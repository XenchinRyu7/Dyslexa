using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Menangani export data semua profil anak ke:
/// - CSV (bisa dibuka di Excel)
/// - HTML report (bisa di-print sebagai PDF dari browser)
/// </summary>
public class DataExportManager : MonoBehaviour
{
    public static DataExportManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // =============================================
    // EXPORT CSV (EXCEL)
    // =============================================

    public void ExportToCSV()
    {
        List<PlayerProfile> profiles = PlayerProfileManager.Instance.GetAllProfiles();

        if (profiles.Count == 0)
        {
            Debug.LogWarning("[DataExport] Tidak ada profil untuk diekspor.");
            return;
        }

        string xlsPath = Path.Combine(Application.persistentDataPath, "Dyslexa_DataExport.xls");

        StringBuilder sb = new StringBuilder();

        // ── XML Spreadsheet 2003 Header ──────────────────
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");

        // Style: header bold + background biru
        sb.AppendLine("<Styles>");
        sb.AppendLine("  <Style ss:ID=\"header\">");
        sb.AppendLine("    <Font ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/>");
        sb.AppendLine("    <Interior ss:Color=\"#2574FF\" ss:Pattern=\"Solid\"/>");
        sb.AppendLine("    <Alignment ss:Horizontal=\"Center\"/>");
        sb.AppendLine("  </Style>");
        sb.AppendLine("  <Style ss:ID=\"number\"><NumberFormat ss:Format=\"0.00\"/></Style>");
        sb.AppendLine("  <Style ss:ID=\"pct\"><NumberFormat ss:Format=\"0%\"/></Style>");
        sb.AppendLine("</Styles>");

        // ════════════════════════════════════════════════
        // SHEET 1 — Data Profil Anak
        // ════════════════════════════════════════════════
        sb.AppendLine("<Worksheet ss:Name=\"Profil Anak\">");
        sb.AppendLine("<Table>");

        // Header row
        string[] profileHeaders = { "No", "Nama", "Umur", "Gender", "Tanggal Dibuat" };
        sb.Append("<Row>");
        foreach (var h in profileHeaders)
            sb.Append($"<Cell ss:StyleID=\"header\"><Data ss:Type=\"String\">{XmlEsc(h)}</Data></Cell>");
        sb.AppendLine("</Row>");

        // Data rows
        int no = 1;
        foreach (var p in profiles)
        {
            sb.AppendLine("<Row>");
            sb.AppendLine($"  <Cell><Data ss:Type=\"Number\">{no++}</Data></Cell>");
            sb.AppendLine($"  <Cell><Data ss:Type=\"String\">{XmlEsc(p.playerName)}</Data></Cell>");
            sb.AppendLine($"  <Cell><Data ss:Type=\"Number\">{p.age}</Data></Cell>");
            sb.AppendLine($"  <Cell><Data ss:Type=\"String\">{XmlEsc(p.gender)}</Data></Cell>");
            sb.AppendLine($"  <Cell><Data ss:Type=\"String\">{XmlEsc(p.creationDate)}</Data></Cell>");
            sb.AppendLine("</Row>");
        }

        sb.AppendLine("</Table></Worksheet>");

        // ════════════════════════════════════════════════
        // SHEET 2 — Riwayat Sesi Permainan
        // ════════════════════════════════════════════════
        sb.AppendLine("<Worksheet ss:Name=\"Riwayat Sesi\">");
        sb.AppendLine("<Table>");

        string[] sessionHeaders =
        {
            "No", "Profile ID", "Nama Pemain", "Level / Node",
            "Akurasi (%)", "Error Rate (%)",
            "Error Fonologis", "Error Visual", "Hint Digunakan",
            "Difficulty Sebelum", "Difficulty Sesudah",
            "Rata Waktu Respons (s)", "Total Waktu Sesi (s)", "Timestamp"
        };

        sb.Append("<Row>");
        foreach (var h in sessionHeaders)
            sb.Append($"<Cell ss:StyleID=\"header\"><Data ss:Type=\"String\">{XmlEsc(h)}</Data></Cell>");
        sb.AppendLine("</Row>");

        string sessionLogPath = Path.Combine(Application.persistentDataPath, "session_logs.json");
        if (File.Exists(sessionLogPath))
        {
            try
            {
                string json = File.ReadAllText(sessionLogPath);
                SessionLogList logs = JsonUtility.FromJson<SessionLogList>(json);

                if (logs != null && logs.sessions != null)
                {
                    int sno = 1;
                    foreach (var s in logs.sessions)
                    {
                        sb.AppendLine("<Row>");
                        sb.AppendLine($"  <Cell><Data ss:Type=\"Number\">{sno++}</Data></Cell>");
                        sb.AppendLine($"  <Cell><Data ss:Type=\"String\">{XmlEsc(s.profileId)}</Data></Cell>");
                        sb.AppendLine($"  <Cell><Data ss:Type=\"String\">{XmlEsc(s.playerName)}</Data></Cell>");
                        sb.AppendLine($"  <Cell><Data ss:Type=\"String\">Level {s.nodeIndex + 1}</Data></Cell>");
                        sb.AppendLine($"  <Cell ss:StyleID=\"pct\"><Data ss:Type=\"Number\">{s.accuracy:F4}</Data></Cell>");
                        sb.AppendLine($"  <Cell ss:StyleID=\"pct\"><Data ss:Type=\"Number\">{s.error_rate:F4}</Data></Cell>");
                        sb.AppendLine($"  <Cell><Data ss:Type=\"Number\">{s.phonology_errors}</Data></Cell>");
                        sb.AppendLine($"  <Cell><Data ss:Type=\"Number\">{s.visual_errors}</Data></Cell>");
                        sb.AppendLine($"  <Cell><Data ss:Type=\"Number\">{s.total_hints_used}</Data></Cell>");
                        sb.AppendLine($"  <Cell><Data ss:Type=\"Number\">{s.difficulty_before}</Data></Cell>");
                        sb.AppendLine($"  <Cell><Data ss:Type=\"Number\">{s.difficulty_after}</Data></Cell>");
                        sb.AppendLine($"  <Cell ss:StyleID=\"number\"><Data ss:Type=\"Number\">{s.avg_response_time:F2}</Data></Cell>");
                        sb.AppendLine($"  <Cell ss:StyleID=\"number\"><Data ss:Type=\"Number\">{s.waktu_penyelesaian:F2}</Data></Cell>");
                        sb.AppendLine($"  <Cell><Data ss:Type=\"String\">{XmlEsc(s.timestamp)}</Data></Cell>");
                        sb.AppendLine("</Row>");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DataExport] Gagal membaca session log: {e.Message}");
            }
        }
        else
        {
            // Baris kosong jika belum ada data sesi
            sb.AppendLine("<Row><Cell><Data ss:Type=\"String\">Belum ada data sesi.</Data></Cell></Row>");
        }

        sb.AppendLine("</Table></Worksheet>");
        sb.AppendLine("</Workbook>");

        try
        {
            File.WriteAllText(xlsPath, sb.ToString(), Encoding.UTF8);
            Debug.Log($"[DataExport] Excel berhasil disimpan di: {xlsPath}");
            OpenInExplorer(xlsPath);
        }
        catch (IOException ex)
        {
            Debug.LogError($"[DataExport] Gagal menyimpan Excel: {ex.Message}");
        }
    }

    // =============================================
    // EXPORT HTML REPORT (PDF-READY)
    // =============================================

    public void ExportToPDF()
    {
        List<PlayerProfile> profiles = PlayerProfileManager.Instance.GetAllProfiles();

        if (profiles.Count == 0)
        {
            Debug.LogWarning("[DataExport] Tidak ada profil untuk diekspor.");
            return;
        }

        string htmlPath = Path.Combine(Application.persistentDataPath, "Dyslexa_Report.html");
        string html = BuildHTMLReport(profiles);

        try
        {
            File.WriteAllText(htmlPath, html, Encoding.UTF8);
            Debug.Log($"[DataExport] HTML Report berhasil disimpan di: {htmlPath}");

            // Buka file di browser default (user bisa Ctrl+P → Save as PDF)
            Application.OpenURL("file://" + htmlPath);
        }
        catch (IOException ex)
        {
            Debug.LogError($"[DataExport] Gagal menyimpan HTML Report: {ex.Message}");
        }
    }

    private string BuildHTMLReport(List<PlayerProfile> profiles)
    {
        StringBuilder sb = new StringBuilder();
        string now = DateTime.Now.ToString("dd MMMM yyyy, HH:mm");

        sb.Append(@"<!DOCTYPE html>
<html lang='id'>
<head>
<meta charset='UTF-8'>
<title>Laporan Data Dyslexa</title>
<style>
  body { font-family: Arial, sans-serif; margin: 40px; color: #222; background: #f9f9f9; }
  h1 { color: #2574FF; border-bottom: 3px solid #2574FF; padding-bottom: 10px; }
  h2 { color: #444; margin-top: 30px; }
  .meta { color: #888; font-size: 13px; margin-bottom: 30px; }
  table { width: 100%; border-collapse: collapse; margin-bottom: 30px; background: white; box-shadow: 0 1px 4px rgba(0,0,0,0.1); }
  th { background: #2574FF; color: white; padding: 10px 14px; text-align: left; font-size: 13px; }
  td { padding: 9px 14px; font-size: 13px; border-bottom: 1px solid #eee; }
  tr:last-child td { border-bottom: none; }
  tr:nth-child(even) td { background: #f4f8ff; }
  .badge-pass { background: #d4f5d4; color: #1a7a1a; border-radius: 4px; padding: 2px 8px; font-size: 12px; }
  .badge-fail { background: #fde8e8; color: #a00; border-radius: 4px; padding: 2px 8px; font-size: 12px; }
  footer { margin-top: 40px; color: #aaa; font-size: 12px; text-align: center; }
  @media print { body { margin: 20px; } }
</style>
</head>
<body>");

        sb.Append($"<h1>Laporan Data Aplikasi Dyslexa</h1>");
        sb.Append($"<p class='meta'>Diekspor pada: {now} &nbsp;|&nbsp; Total Profil: {profiles.Count}</p>");

        // --- TABEL PROFIL ---
        sb.Append("<h2>Data Profil Anak</h2>");
        sb.Append("<table><tr><th>#</th><th>Nama</th><th>Umur</th><th>Gender</th><th>Tanggal Dibuat</th></tr>");

        for (int i = 0; i < profiles.Count; i++)
        {
            var p = profiles[i];
            sb.Append($"<tr><td>{i + 1}</td><td>{p.playerName}</td><td>{p.age} tahun</td><td>{p.gender}</td><td>{p.creationDate}</td></tr>");
        }
        sb.Append("</table>");

        // --- TABEL SESI ---
        string sessionLogPath = Path.Combine(Application.persistentDataPath, "session_logs.json");
        if (File.Exists(sessionLogPath))
        {
            try
            {
                string json = File.ReadAllText(sessionLogPath);
                SessionLogList logs = JsonUtility.FromJson<SessionLogList>(json);

                if (logs != null && logs.sessions != null && logs.sessions.Count > 0)
                {
                    sb.Append("<h2>Riwayat Sesi Permainan</h2>");
                    sb.Append("<table><tr><th>Node</th><th>Akurasi</th><th>Error Rate</th><th>Err Fonologis</th><th>Err Visual</th><th>Diff Sebelum</th><th>Diff Sesudah</th><th>Rata Waktu (s)</th><th>Total Waktu (s)</th><th>Timestamp</th></tr>");

                    foreach (var s in logs.sessions)
                    {
                        string passClass = s.accuracy >= 0.8f ? "badge-pass" : "badge-fail";
                        string passLabel = s.accuracy >= 0.8f ? "Lulus" : "Belum";

                        sb.Append(
                            $"<tr>" +
                            $"<td>Level {s.nodeIndex + 1}</td>" +
                            $"<td><span class='{passClass}'>{s.accuracy:P0} {passLabel}</span></td>" +
                            $"<td>{s.error_rate:P0}</td>" +
                            $"<td>{s.phonology_errors}</td>" +
                            $"<td>{s.visual_errors}</td>" +
                            $"<td>{s.difficulty_before}</td>" +
                            $"<td>{s.difficulty_after}</td>" +
                            $"<td>{s.avg_response_time:F1}s</td>" +
                            $"<td>{s.waktu_penyelesaian:F1}s</td>" +
                            $"<td>{s.timestamp}</td>" +
                            $"</tr>"
                        );
                    }
                    sb.Append("</table>");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DataExport] Gagal membaca session log untuk HTML: {e.Message}");
            }
        }

        sb.Append("<footer>Laporan dibuat otomatis oleh Aplikasi Dyslexa &mdash; Untuk menyimpan sebagai PDF, tekan Ctrl+P di browser lalu pilih 'Save as PDF'</footer>");
        sb.Append("</body></html>");

        return sb.ToString();
    }

    // =============================================
    // HELPER
    // =============================================

    /// <summary>Escape karakter spesial XML supaya tidak rusak struktur file.</summary>
    private string XmlEsc(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value
            .Replace("&",  "&amp;")
            .Replace("<",  "&lt;")
            .Replace(">",  "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'",  "&apos;");
    }

    private void OpenInExplorer(string filePath)
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.RevealInFinder(filePath);
#elif UNITY_STANDALONE_WIN
        // Buka folder tempat file disimpan di Windows
        string folder = Path.GetDirectoryName(filePath);
        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
#endif
        Debug.Log($"[DataExport] File disimpan di: {filePath}");
    }
}
