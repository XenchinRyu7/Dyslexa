using UnityEngine;

[System.Serializable]
public class SessionMetrics
{
    public int jumlah_benar;
    public int jumlah_salah;
    public float rata_waktu_respons;
    public int kesalahan_fonologis;
    public int kesalahan_visual;
    public int penggunaan_hint;
    public int total_soal;
    public float waktu_penyelesaian; // Total session completion time

    // Derived metrics
    public float accuracy;
    public float error_rate;
    public float hint_rate;
    public float fonologis_rate;
    public float visual_rate;

    public void CalculateDerivedMetrics()
    {
        accuracy = total_soal > 0 ? (float)jumlah_benar / total_soal : 0f;
        error_rate = total_soal > 0 ? (float)jumlah_salah / total_soal : 0f;
        hint_rate = total_soal > 0 ? (float)penggunaan_hint / total_soal : 0f;
        fonologis_rate = jumlah_salah > 0 ? (float)kesalahan_fonologis / jumlah_salah : 0f;
        visual_rate = jumlah_salah > 0 ? (float)kesalahan_visual / jumlah_salah : 0f;
    }
}
