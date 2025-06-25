using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

/// <summary>
/// Records VR gestures using a controller and renders them onto a texture for classification.
/// </summary>
public class VRGestureRecorder : MonoBehaviour
{
    /// <summary>
    /// The VR controller used for recording gestures.
    /// </summary>
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    /// <summary>
    /// The UI element displaying the gesture canvas.
    /// </summary>
    public RawImage gestureCanvas;

    /// <summary>
    /// The gesture classifier used to classify recorded gestures.
    /// </summary>
    public GestureSynthGrayClassifier gestureClassifier;

    /// <summary>
    /// The size of the texture used for rendering gestures.
    /// </summary>
    const int TEX_SIZE = 216;

    /// <summary>
    /// The radius of the brush used for drawing gestures.
    /// </summary>
    const int BRUSH_RADIUS = 2;

    /// <summary>
    /// The color of the brush used for drawing gestures.
    /// </summary>
    readonly Color BRUSH_COLOR = Color.white;

    /// <summary>
    /// The texture used for rendering gestures.
    /// </summary>
    private Texture2D drawingTexture;

    /// <summary>
    /// The raw points recorded from the VR controller.
    /// </summary>
    private List<Vector3> rawPoints = new();

    /// <summary>
    /// Indicates whether the recorder is currently recording a gesture.
    /// </summary>
    private bool isRecording = false;

    /// <summary>
    /// Initializes the drawing texture and clears it.
    /// </summary>
    void Start()
    {
        drawingTexture = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
        drawingTexture.filterMode = FilterMode.Point;
        drawingTexture.wrapMode = TextureWrapMode.Clamp;
        ClearTexture();
        gestureCanvas.texture = drawingTexture;
    }

    /// <summary>
    /// Updates the recorder state, handles input, and processes gestures.
    /// </summary>
    void Update()
    {
        if (GameManager.instance == null || !GameManager.instance.CanDraw)
        {
            isRecording = false;
            return; // Don't record if the game is over or cooldown is active
        }
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
        {
            isRecording = true;
            rawPoints.Clear();
            ClearTexture();
        }
        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller))
        {
            isRecording = false;
            DrawStroke();
            gestureClassifier.Classify();
        }
        if (isRecording)
            rawPoints.Add(OVRInput.GetLocalControllerPosition(controller));
    }

    /// <summary>
    /// Clears the drawing texture by filling it with a black color.
    /// </summary>
    void ClearTexture()
    {
        var cols = drawingTexture.GetPixels32();
        for (int i = 0; i < cols.Length; i++)
            cols[i] = Color.black;
        drawingTexture.SetPixels32(cols);
        drawingTexture.Apply();
    }

    /// <summary>
    /// Draws the recorded stroke onto the texture by mapping 3D points to 2D space.
    /// </summary>
    void DrawStroke()
    {
        if (rawPoints.Count < 2) return;
        // mapowanie rawPoints do przestrzeni 2D canvasa
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        var pts2d = new Vector2[rawPoints.Count];
        for (int i = 0; i < rawPoints.Count; i++)
        {
            var p = rawPoints[i];
            pts2d[i] = new Vector2(p.x, p.y);
            minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
        }
        // skalowanie do 0–TEX_SIZE
        float scaleX = (TEX_SIZE - 2f * BRUSH_RADIUS) / (maxX - minX + 1e-5f);
        float scaleY = (TEX_SIZE - 2f * BRUSH_RADIUS) / (maxY - minY + 1e-5f);

        for (int i = 1; i < pts2d.Length; i++)
        {
            int x0 = Mathf.RoundToInt((pts2d[i - 1].x - minX) * scaleX) + BRUSH_RADIUS;
            int y0 = Mathf.RoundToInt((pts2d[i - 1].y - minY) * scaleY) + BRUSH_RADIUS;
            int x1 = Mathf.RoundToInt((pts2d[i].x - minX) * scaleX) + BRUSH_RADIUS;
            int y1 = Mathf.RoundToInt((pts2d[i].y - minY) * scaleY) + BRUSH_RADIUS;
            DrawThickLine(x0, y0, x1, y1);
        }

        drawingTexture.Apply();
    }

    /// <summary>
    /// Draws a thick line between two points on the texture.
    /// </summary>
    /// <param name="x0">The x-coordinate of the starting point.</param>
    /// <param name="y0">The y-coordinate of the starting point.</param>
    /// <param name="x1">The x-coordinate of the ending point.</param>
    /// <param name="y1">The y-coordinate of the ending point.</param>
    void DrawThickLine(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        while (true)
        {
            DrawThickPixel(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = err * 2;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    /// <summary>
    /// Draws a thick pixel at the specified position on the texture.
    /// </summary>
    /// <param name="cx">The x-coordinate of the pixel.</param>
    /// <param name="cy">The y-coordinate of the pixel.</param>
    void DrawThickPixel(int cx, int cy)
    {
        for (int oy = -BRUSH_RADIUS; oy <= BRUSH_RADIUS; oy++)
            for (int ox = -BRUSH_RADIUS; ox <= BRUSH_RADIUS; ox++)
            {
                int x = cx + ox, y = cy + oy;
                if (x >= 0 && y >= 0 && x < TEX_SIZE && y < TEX_SIZE)
                    drawingTexture.SetPixel(x, y, BRUSH_COLOR);
            }
    }
}
