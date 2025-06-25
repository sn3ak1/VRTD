using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class GestureSynthGrayClassifier : MonoBehaviour
{
    [Header("ONNX Model (synth_gray)")]
    public NNModel onnxModel;

    [Header("UI References")]
    public RawImage gestureCanvas;
    public TextMeshProUGUI outputText;

    int _H = 216, _W = 216;
    IWorker _worker;
    readonly string[] _labels = { "circle", "loop", "s", "spiral", "w" };

    void Start()
    {
        var model = ModelLoader.Load(onnxModel);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);
    }

    void OnDestroy()
    {
        _worker?.Dispose();
    }

    public void Classify()
    {
        var srcTex = gestureCanvas.texture as Texture2D;
        if (srcTex == null)
        {
            outputText.text = "No texture";
            return;
        }

        string dir = Path.Combine(Application.persistentDataPath, "GestureDumps");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        string ts = System.DateTime.Now.ToString("yyyyMMdd_HHmmssfff");

        string rawPath = Path.Combine(dir, $"gesture_{ts}_raw.png");
        File.WriteAllBytes(rawPath, srcTex.EncodeToPNG());
        Debug.Log($"[GestureDump] Raw → {rawPath}");


        var px = srcTex.GetPixels32();
        int N = _W * _H * 3;
        float[] data = new float[N];
        for (int i = 0; i < px.Length; i++)
        {
            data[i * 3 + 0] = px[i].r / 255f;
            data[i * 3 + 1] = px[i].g / 255f;
            data[i * 3 + 2] = px[i].b / 255f;
        }

        using (var t = new Tensor(1, _H, _W, 3, data))
        {
            _worker.Execute(t);
            var outT = _worker.PeekOutput();
            int cls = outT.ArgMax()[0];
            float prob = outT[0, cls];
            outputText.text = $"{_labels[cls]} ({prob * 100f:0.0}%)";
            outT.Dispose();
        }
        GameManager.instance.HandleGestureInput(ClassName(classIndex));
    }
}
