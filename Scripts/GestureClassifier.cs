using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using System.Collections;

/// <summary>
/// Classifies gestures using a pre-trained ONNX model and updates the game state based on the classification results.
/// </summary>
public class GestureSynthGrayClassifier : MonoBehaviour
{
    /// <summary>
    /// The ONNX model used for gesture classification.
    /// </summary>
    [Header("ONNX Model (synth_gray)")]
    public NNModel onnxModel;

    /// <summary>
    /// The UI element displaying the gesture canvas.
    /// </summary>
    [Header("UI References")]
    public RawImage gestureCanvas;

    /// <summary>
    /// The UI element displaying the classification output.
    /// </summary>
    public TextMeshProUGUI outputText;

    /// <summary>
    /// The height and width of the input texture for the model.
    /// </summary>
    int _H = 216, _W = 216;

    /// <summary>
    /// The worker used to execute the ONNX model.
    /// </summary>
    IWorker _worker;

    /// <summary>
    /// The labels corresponding to the output classes of the model.
    /// </summary>
    readonly string[] _labels = { "circle", "loop", "s", "spiral", "w" };

    /// <summary>
    /// Initializes the ONNX model and creates a worker for inference.
    /// </summary>
    void Start()
    {
        var model = ModelLoader.Load(onnxModel);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model);
    }

    /// <summary>
    /// Disposes of the worker when the object is destroyed.
    /// </summary>
    void OnDestroy()
    {
        _worker?.Dispose();
    }

    /// <summary>
    /// Reads the texture from a RawImage, supporting both Texture2D and RenderTexture types.
    /// </summary>
    /// <param name="rawImage">The RawImage to read the texture from.</param>
    /// <returns>The extracted Texture2D, or null if the texture type is unsupported.</returns>
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

    /// <summary>
    /// Initiates the gesture classification process and updates the cooldown timer.
    /// </summary>
    public void Classify()
    {
        GameManager.instance.currDrawingCooldown = GameManager.instance.drawingCooldown;
        StartCoroutine(RunInferenceAsync());
    }

    /// <summary>
    /// Runs the gesture classification asynchronously and updates the game state based on the result.
    /// </summary>
    /// <returns>An enumerator for coroutine execution.</returns>
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

        yield return null;

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
