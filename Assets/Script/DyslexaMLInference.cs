using System;
using System.IO;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

/// <summary>
/// Random Forest inference via Microsoft ONNX Runtime (ORT).
/// Package: com.github.asus4.onnxruntime (via NPM scoped registry)
///
/// ORT mendukung TreeEnsembleClassifier — beda dengan Unity Sentis
/// yang hanya support neural network operators.
///
/// Setup:
///   1. Tambah ke manifest.json: com.github.asus4.onnxruntime
///   2. Copy dyslexa_rf_model.onnx ke Assets/StreamingAssets/
///   3. Attach script ini ke GameObject HomeScreen/MainScene
/// </summary>
public class DyslexaMLInference : MonoBehaviour
{
    public static DyslexaMLInference Instance { get; private set; }

    [Header("Model")]
    [Tooltip("Nama file .onnx di StreamingAssets")]
    public string modelFileName = "dyslexa_rf_model.onnx";

    // LabelEncoder mapping: output index → difficulty change
    // le.classes_ = [-1, 0, 1] → index 0=-1, 1=0, 2=+1
    private readonly int[] _labelMap = { -1, 0, 1 };

    private InferenceSession _session;
    private bool _isReady = false;

    // ── Lifecycle ─────────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadModel();
        }
        else Destroy(gameObject);
    }

    void OnDestroy() => _session?.Dispose();

    private void LoadModel()
    {
        string path = Path.Combine(Application.streamingAssetsPath, modelFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"[RF] Model tidak ditemukan: {path}\n" +
                           "Copy dyslexa_rf_model.onnx ke Assets/StreamingAssets/");
            return;
        }

        try
        {
            var opts = new SessionOptions();
            opts.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING;
            _session  = new InferenceSession(path, opts);
            _isReady  = true;
            Debug.Log("[RF] ✅ Random Forest loaded via ONNX Runtime");
        }
        catch (Exception e)
        {
            Debug.LogError($"[RF] Gagal load model: {e.Message}");
        }
    }

    // ── Predict ───────────────────────────────────────────────────

    /// <summary>
    /// Prediksi perubahan difficulty dari SessionMetrics.
    /// Return: -1 (turun), 0 (tetap), +1 (naik)
    /// </summary>
    public int Predict(SessionMetrics metrics, int currentDifficulty)
    {
        if (!_isReady)
        {
            Debug.LogWarning("[RF] Model belum ready, fallback ke Rule Engine.");
            return RuleEngineFallback(metrics);
        }

        try
        {
            // Input order sesuai FEATURES di training script (8 fitur):
            float[] inputData = {
                metrics.accuracy,
                metrics.error_rate,
                metrics.kesalahan_fonologis,
                metrics.kesalahan_visual,
                metrics.penggunaan_hint,
                metrics.rata_waktu_respons,
                metrics.waktu_penyelesaian,       // fitur ke-7: total durasi sesi
                (float)currentDifficulty           // fitur ke-8
            };

            // Buat input tensor shape [1, 8]
            var tensor = new DenseTensor<float>(inputData, new[] { 1, 8 });
            var inputs = new[]
            {
                NamedOnnxValue.CreateFromTensor("input", tensor)
            };

            using var results = _session.Run(inputs);

            // Output dengan zipmap=False:
            // "variable" → int64 tensor [1] = predicted class index
            var labelTensor = results[0].AsTensor<long>();
            int classIdx    = (int)labelTensor[0];

            int change = (classIdx >= 0 && classIdx < _labelMap.Length)
                ? _labelMap[classIdx] : 0;

            Debug.Log($"[RF] class={classIdx} → change={change:+0;-0;0} " +
                      $"(acc={metrics.accuracy:P0}, waktu={metrics.waktu_penyelesaian:F0}s, diff={currentDifficulty})");
            return change;
        }
        catch (Exception e)
        {
            Debug.LogError($"[RF] Inference error: {e.Message}");
            return RuleEngineFallback(metrics);
        }
    }

    // ── Fallback (hanya jika model gagal load) ────────────────────

    private int RuleEngineFallback(SessionMetrics metrics)
    {
        if (metrics.accuracy >= 0.85f && metrics.hint_rate < 0.2f) return +1;
        if (metrics.accuracy  < 0.60f)                              return -1;
        return 0;
    }
}
