using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles drawing in 3D space using a VR controller, creating tube-like meshes to represent the drawing.
/// </summary>
public class XRControllerDraw : MonoBehaviour
{
    /// <summary>
    /// The transform of the VR controller used for drawing.
    /// </summary>
    [SerializeField] private Transform controllerTransform;

    /// <summary>
    /// The trigger threshold value to start drawing.
    /// </summary>
    [SerializeField] private float triggerThreshold = 0.5f;

    /// <summary>
    /// The minimum distance required between points to add a new point to the drawing.
    /// </summary>
    [SerializeField] private float minDistanceBeforeNewPoint = 0.008f;

    /// <summary>
    /// The default width of the tube representing the drawing.
    /// </summary>
    [SerializeField] private float tubeDefaultWidth = 0.010f;

    /// <summary>
    /// The number of sides for the tube's cross-section.
    /// </summary>
    [SerializeField] private int tubeSides = 8;

    /// <summary>
    /// The default color of the tube material.
    /// </summary>
    [SerializeField] private Color defaultColor = Color.white;

    /// <summary>
    /// The default material used for the tube.
    /// </summary>
    [SerializeField] private Material defaultLineMaterial;

    /// <summary>
    /// The position of the previous point in the drawing.
    /// </summary>
    private Vector3 prevPointPosition = Vector3.zero;

    /// <summary>
    /// The list of points that define the current drawing.
    /// </summary>
    private List<Vector3> points = new List<Vector3>();

    /// <summary>
    /// The list of tube renderers created for the drawings.
    /// </summary>
    private List<TubeRenderer> tubeRenderers = new List<TubeRenderer>();

    /// <summary>
    /// The current tube renderer used for the ongoing drawing.
    /// </summary>
    private TubeRenderer currentTubeRenderer;

    /// <summary>
    /// Indicates whether the user is currently drawing.
    /// </summary>
    private bool isDrawing = false;

    /// <summary>
    /// Initializes the first tube renderer at the start of the application.
    /// </summary>
    private void Start()
    {
        AddNewTubeRenderer();
    }

    /// <summary>
    /// Updates the drawing state based on controller input and manages the drawing process.
    /// </summary>
    private void Update()
    {
        // ADDED
        if (GameManager.instance == null || !GameManager.instance.CanDraw)
        {
            return; // Don't record if the game is over or cooldown is active
        }

        float triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger); // Right controller trigger

        if (triggerValue >= triggerThreshold)
        {
            UpdateTube();
            isDrawing = true;
        }
        else if (isDrawing)
        {
            Destroy(currentTubeRenderer.gameObject); // Destroy the current tube renderer's GameObject
            AddNewTubeRenderer();
            isDrawing = false;
        }
    }

    /// <summary>
    /// Adds a new tube renderer for a new drawing session.
    /// </summary>
    private void AddNewTubeRenderer()
    {
        points.Clear();
        GameObject go = new GameObject($"TubeRenderer__{tubeRenderers.Count}");
        go.transform.position = Vector3.zero;

        TubeRenderer goTubeRenderer = go.AddComponent<TubeRenderer>();
        tubeRenderers.Add(goTubeRenderer);

        var renderer = go.GetComponent<MeshRenderer>();
        renderer.material = defaultLineMaterial;

        goTubeRenderer.SetPositions(points.ToArray());
        goTubeRenderer._radiusOne = tubeDefaultWidth;
        goTubeRenderer._radiusTwo = tubeDefaultWidth;
        goTubeRenderer._sides = tubeSides;

        currentTubeRenderer = goTubeRenderer;
        prevPointPosition = Vector3.zero;
    }

    /// <summary>
    /// Updates the current tube by adding new points based on the controller's position.
    /// </summary>
    private void UpdateTube()
    {
        if (controllerTransform == null)
            return;

        Vector3 currentPosition = controllerTransform.position;

        if (prevPointPosition == Vector3.zero || Vector3.Distance(prevPointPosition, currentPosition) >= minDistanceBeforeNewPoint)
        {
            prevPointPosition = currentPosition;
            AddPoint(currentPosition);
        }
    }

    /// <summary>
    /// Adds a new point to the current drawing and updates the tube mesh.
    /// </summary>
    /// <param name="position">The position of the new point.</param>
    private void AddPoint(Vector3 position)
    {
        points.Add(position);
        currentTubeRenderer.SetPositions(points.ToArray());
        currentTubeRenderer.GenerateMesh();
    }

}
