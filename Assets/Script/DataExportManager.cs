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
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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

        string csvPath = Path.Combine(Application.persistentDataPath, "Dyslexa_DataExport.csv");
        StringBuilder sb = new StringBuilder();

        // Header
        sb.AppendLine("ProfileID,Nama,Umur,Gender,Tanggal Dibuat");

        // Data per profil
        foreach (var profile in profiles)
        {
            sb.AppendLine($"{profile.profileId},{EscapeCSV(profile.playerName)},{profile.age},{EscapeCSV(profile.gender)},{EscapeCSV(profile.creationDate)}");
        }

        // Tambah session log per profil jika ada
        string sessionLogPath = Path.Combine(Application.persistentDataPath, "session_logs.json");
        if (File.Exists(sessionLogPath))
        {
            sb.AppendLine(); // Baris kosong pemisah
            sb.AppendLine("--- DATA SESI ---");
            sb.AppendLine("Node,Akurasi,Error Rate,Error Fonologis,Error Visual,Difficulty Sebelum,Difficulty Sesudah,Rata Waktu Respons (s),Waktu Penyelesaian (s),Timestamp");

            try
            {
                string json = File.ReadAllText(sessionLogPath);
                SessionLogList logs = JsonUtility.FromJson<SessionLogList>(json);

                if (logs != null && logs.sessions != null)
                {
                    foreach (var session in logs.sessions)
                    {
                        sb.AppendLine(
                            $"{session.nodeIndex + 1}," +
                            $"{session.accuracy:P0}," +
                            $"{session.error_rate:P0}," +
                            $"{session.phonology_errors}," +
                            $"{session.visual_errors}," +
                            $"{session.difficulty_before}," +
                            $"{session.difficulty_after}," +
                            $"{session.avg_response_time:F1}," +
                            $"{session.waktu_penyelesaian:F1}," +
                            $"{session.timestamp}"
                        );
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DataExport] Gagal membaca session log: {e.Message}");
            }
        }

        try
        {
            File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);
            Debug.Log($"[DataExport] CSV berhasil disimpan di: {csvPath}");
            OpenInExplorer(csvPath);
        }
        catch (IOException ex)
        {
            Debug.LogError($"[DataExport] Gagal menyimpan CSV: {ex.Message}");
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

    private string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        // Jika ada koma/kutip, bungkus dengan tanda kutip
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
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
