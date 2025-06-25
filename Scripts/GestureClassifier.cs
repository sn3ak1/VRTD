using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using System.Collections;

public class GestureSynthGrayClassifier : MonoBehaviour
{
    [Header("ONNX Model (synth_gray)")]
    public NNModel onnxModel;

    [Header("UI References")]
    public RawImage gestureCanvas;
    public TextMeshProUGUI outputText;

    // ustawione na 216×216 – takie same jak texture w VRGestureRecorder
    int _H = 216, _W = 216;
    IWorker _worker;
    readonly string[] _labels = { "circle", "loop", "s", "spiral", "w" };

    void Start()
    {
        var model = ModelLoader.Load(onnxModel);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model);
    }

    void OnDestroy()
    {
        _worker?.Dispose();
    }

    Texture2D ReadTextureFromRawImage(RawImage rawImage)
    {
        if (rawImage.texture is Texture2D tex2D)
            return tex2D;

        if (rawImage.texture is RenderTexture rt)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = prev;
            return tex;
        }

        Debug.LogWarning("Unknown texture type on RawImage!");
        return null;
    }


    public void Classify()
    {
        GameManager.instance.currDrawingCooldown = GameManager.instance.drawingCooldown;
        StartCoroutine(RunInferenceAsync());
    }

    IEnumerator RunInferenceAsync()
    {
        var srcTex = gestureCanvas.texture as Texture2D;
        if (srcTex == null)
        {
            outputText.text = "No texture";
            yield break;
        }

        var px = srcTex.GetPixels32();
        float[] data = new float[_W * _H * 3];
        for (int i = 0; i < px.Length; i++)
        {
            data[i * 3 + 0] = px[i].r / 255f;
            data[i * 3 + 1] = px[i].g / 255f;
            data[i * 3 + 2] = px[i].b / 255f;
        }

        yield return null; // Let Unity breathe

        Tensor t = new Tensor(1, _H, _W, 3, data);
        _worker.Execute(t);
        var outT = _worker.PeekOutput();

        int cls = outT.ArgMax()[0];
        float prob = outT[0, cls];
        outputText.text = $"{_labels[cls]} ({prob * 100f:0.0}%)";

        t.Dispose();
        outT.Dispose();

        GameManager.instance.HandleGestureInput(prob > 0.3f ? _labels[cls] : "unknown");
    }

}
