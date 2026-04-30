using UnityEngine;

/// <summary>
/// Random Forest inference via Unity Sentis.
/// Model diekspor dengan skl2onnx options={'zipmap': False}
/// sehingga output berupa Tensor int64 (bukan Sequence of Maps).
///
/// Setup:
///   1. python train_dyslexa_model.py → dyslexa_rf_model.onnx
///   2. Copy ke Assets/Resources/ML/dyslexa_rf_model.onnx
///   3. Install: com.unity.sentis
///   4. Attach ke GameObject HomeScreen/MainScene
/// </summary>
public class DyslexaMLInference : MonoBehaviour
{
    public static DyslexaMLInference Instance { get; private set; }

    [Header("Model")]
    [Tooltip("Path di Resources/ tanpa ekstensi")]
    public string modelResourcePath = "ML/dyslexa_rf_model";

    // classes dari LabelEncoder: index 0→-1, 1→0, 2→+1
    private readonly int[] _labelMap = { -1, 0, 1 };
    private bool _isReady = false;

#if UNITY_SENTIS
    private Unity.Sentis.Model  _model;
    private Unity.Sentis.Worker _worker;
#endif

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); LoadModel(); }
        else Destroy(gameObject);
    }

    void OnDestroy()
    {
#if UNITY_SENTIS
        _worker?.Dispose();
#endif
    }

    private void LoadModel()
    {
#if UNITY_SENTIS
        var asset = Resources.Load<Unity.Sentis.ModelAsset>(modelResourcePath);
        if (asset == null)
        {
            Debug.LogWarning($"[RF] Model tidak ditemukan di Resources/{modelResourcePath}");
            return;
        }
        _model   = Unity.Sentis.ModelLoader.Load(asset);
        _worker  = new Unity.Sentis.Worker(_model, Unity.Sentis.BackendType.CPU);
        _isReady = true;
        Debug.Log("[RF] ✅ Random Forest loaded via Sentis");
#endif
    }

    public int Predict(SessionMetrics metrics, int currentDifficulty)
    {
#if UNITY_SENTIS
        if (_isReady)
        {
            float[] input = {
                metrics.accuracy,
                metrics.error_rate,
                metrics.kesalahan_fonologis,
                metrics.kesalahan_visual,
                metrics.penggunaan_hint,
                metrics.rata_waktu_respons,
                (float)currentDifficulty
            };

            using var t = new Unity.Sentis.Tensor<float>(
                new Unity.Sentis.TensorShape(1, 7), input);
            _worker.Schedule(t);

            // output_label → int64 Tensor shape [1] (karena zipmap=False)
            using var label = _worker.PeekOutput("variable") as Unity.Sentis.Tensor<int>;
            if (label != null)
            {
                label.MakeReadable();
                int classIdx = label[0];
                // classIdx adalah index setelah LabelEncoder: 0→-1, 1→0, 2→+1
                int change = (classIdx >= 0 && classIdx < _labelMap.Length)
                    ? _labelMap[classIdx] : 0;

                Debug.Log($"[RF] class={classIdx} → change={change:+0;-0;0}");
                return change;
            }
        }
#endif
        return RuleEngineFallback(metrics);
    }

    private int RuleEngineFallback(SessionMetrics metrics)
    {
        if (metrics.accuracy >= 0.85f && metrics.hint_rate < 0.2f) return +1;
        if (metrics.accuracy  < 0.60f)                              return -1;
        return 0;
    }
}
