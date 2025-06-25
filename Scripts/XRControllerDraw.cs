using System.Collections.Generic;
using UnityEngine;

public class XRControllerDraw : MonoBehaviour
{
    [SerializeField] private Transform controllerTransform;

    [SerializeField] private float triggerThreshold = 0.5f;

    [SerializeField] private float minDistanceBeforeNewPoint = 0.008f;

    [SerializeField] private float tubeDefaultWidth = 0.010f;
    [SerializeField] private int tubeSides = 8;

    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Material defaultLineMaterial;

    private Vector3 prevPointPosition = Vector3.zero;

    private List<Vector3> points = new List<Vector3>();
    private List<TubeRenderer> tubeRenderers = new List<TubeRenderer>();

    private TubeRenderer currentTubeRenderer;
    private bool isDrawing = false;

    private void Start()
    {
        AddNewTubeRenderer();
    }

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

    private void AddPoint(Vector3 position)
    {
        points.Add(position);
        currentTubeRenderer.SetPositions(points.ToArray());
        currentTubeRenderer.GenerateMesh();
    }

}
