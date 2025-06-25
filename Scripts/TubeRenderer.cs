using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeRenderer : MonoBehaviour
{
    /// <summary>
    /// The positions that define the path of the tube.
    /// </summary>
    [SerializeField] private Vector3[] _positions;

    /// <summary>
    /// The number of sides for the tube's cross-section.
    /// </summary>
    [SerializeField] public int _sides;

    /// <summary>
    /// The radius of the tube at one end.
    /// </summary>
    [SerializeField] public float _radiusOne;

    /// <summary>
    /// The radius of the tube at the other end (used if _useTwoRadii is true).
    /// </summary>
    [SerializeField] public float _radiusTwo;

    /// <summary>
    /// Whether the tube's vertices are defined in world space.
    /// </summary>
    [SerializeField] private bool _useWorldSpace = true;

    /// <summary>
    /// Whether to interpolate between two radii along the tube's length.
    /// </summary>
    [SerializeField] private bool _useTwoRadii = false;

    /// <summary>
    /// The vertices of the tube mesh.
    /// </summary>
    private Vector3[] _vertices;

    /// <summary>
    /// The mesh representing the tube.
    /// </summary>
    private Mesh _mesh;

    /// <summary>
    /// The MeshFilter component used to render the tube.
    /// </summary>
    private MeshFilter _meshFilter;

    /// <summary>
    /// The MeshRenderer component used to render the tube.
    /// </summary>
    private MeshRenderer _meshRenderer;

    /// <summary>
    /// The MeshCollider component used for collision detection.
    /// </summary>
    private MeshCollider _meshCollider;

    /// <summary>
    /// Gets or sets the material used to render the tube.
    /// </summary>
    public Material material
    {
        get { return _meshRenderer.material; }
        set { _meshRenderer.material = value; }
    }

    /// <summary>
    /// Initializes the tube renderer components and sets up the mesh and collider.
    /// </summary>
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null)
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
        }
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;

        _meshCollider = GetComponent<MeshCollider>();
        if (_meshCollider == null)
        {
            _meshCollider = gameObject.AddComponent<MeshCollider>();
            _meshCollider.sharedMesh = _mesh;
            _meshCollider.convex = true;
        }
    }

    /// <summary>
    /// Enables the MeshRenderer when the object is enabled.
    /// </summary>
    private void OnEnable()
    {
        _meshRenderer.enabled = true;
    }

    /// <summary>
    /// Disables the MeshRenderer when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        _meshRenderer.enabled = false;
    }

    /// <summary>
    /// Ensures the number of sides is at least 3 when modified in the Inspector.
    /// </summary>
    private void OnValidate()
    {
        _sides = Mathf.Max(3, _sides);
    }

    /// <summary>
    /// Sets the positions that define the tube's path and regenerates the mesh.
    /// </summary>
    /// <param name="positions">The positions defining the tube's path.</param>
    public void SetPositions(Vector3[] positions)
    {
        _positions = positions;
        GenerateMesh();
    }

    /// <summary>
    /// Generates the tube mesh based on the current positions and parameters.
    /// </summary>
    public void GenerateMesh()
    {
        if (_mesh == null || _positions == null || _positions.Length <= 1)
        {
            _mesh = new Mesh();
            return;
        }

        var verticesLength = _sides * _positions.Length;
        if (_vertices == null || _vertices.Length != verticesLength)
        {
            _vertices = new Vector3[verticesLength];

            var indices = GenerateIndices();
            var uvs = GenerateUVs();

            if (verticesLength > _mesh.vertexCount)
            {
                _mesh.vertices = _vertices;
                _mesh.triangles = indices;
                _mesh.uv = uvs;
            }
            else
            {
                _mesh.triangles = indices;
                _mesh.vertices = _vertices;
                _mesh.uv = uvs;
            }
        }

        var currentVertIndex = 0;

        for (int i = 0; i < _positions.Length; i++)
        {
            var circle = CalculateCircle(i);
            foreach (var vertex in circle)
            {
                _vertices[currentVertIndex++] = _useWorldSpace ? transform.InverseTransformPoint(vertex) : vertex;
            }
        }

        _mesh.vertices = _vertices;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshFilter.mesh = _mesh;
        _meshCollider.sharedMesh = _mesh;
    }

    /// <summary>
    /// Generates the UV coordinates for the tube mesh.
    /// </summary>
    /// <returns>An array of UV coordinates.</returns>
    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[_positions.Length * _sides];

        for (int segment = 0; segment < _positions.Length; segment++)
        {
            for (int side = 0; side < _sides; side++)
            {
                var vertIndex = (segment * _sides + side);
                var u = side / (_sides - 1f);
                var v = segment / (_positions.Length - 1f);

                uvs[vertIndex] = new Vector2(u, v);
            }
        }

        return uvs;
    }

    /// <summary>
    /// Generates the indices for the tube mesh triangles.
    /// </summary>
    /// <returns>An array of triangle indices.</returns>
    private int[] GenerateIndices()
    {
        // Two triangles and 3 vertices
        var indices = new int[_positions.Length * _sides * 2 * 3];

        var currentIndicesIndex = 0;
        for (int segment = 1; segment < _positions.Length; segment++)
        {
            for (int side = 0; side < _sides; side++)
            {
                var vertIndex = (segment * _sides + side);
                var prevVertIndex = vertIndex - _sides;

                // Triangle one
                indices[currentIndicesIndex++] = prevVertIndex;
                indices[currentIndicesIndex++] = (side == _sides - 1) ? (vertIndex - (_sides - 1)) : (vertIndex + 1);
                indices[currentIndicesIndex++] = vertIndex;

                // Triangle two
                indices[currentIndicesIndex++] = (side == _sides - 1) ? (prevVertIndex - (_sides - 1)) : (prevVertIndex + 1);
                indices[currentIndicesIndex++] = (side == _sides - 1) ? (vertIndex - (_sides - 1)) : (vertIndex + 1);
                indices[currentIndicesIndex++] = prevVertIndex;
            }
        }

        return indices;
    }

    /// <summary>
    /// Calculates the vertices of a circular cross-section at a given position along the tube.
    /// </summary>
    /// <param name="index">The index of the position along the tube.</param>
    /// <returns>An array of vertices forming the circular cross-section.</returns>
    private Vector3[] CalculateCircle(int index)
    {
        var dirCount = 0;
        var forward = Vector3.zero;

        // If not first index
        if (index > 0)
        {
            forward += (_positions[index] - _positions[index - 1]).normalized;
            dirCount++;
        }

        // If not last index
        if (index < _positions.Length - 1)
        {
            forward += (_positions[index + 1] - _positions[index]).normalized;
            dirCount++;
        }

        // Forward is the average of the connecting edges directions
        forward = (forward / dirCount).normalized;
        var side = Vector3.Cross(forward, forward + new Vector3(.123564f, .34675f, .756892f)).normalized;
        var up = Vector3.Cross(forward, side).normalized;

        var circle = new Vector3[_sides];
        var angle = 0f;
        var angleStep = (2 * Mathf.PI) / _sides;

        var t = index / (_positions.Length - 1f);
        var radius = _useTwoRadii ? Mathf.Lerp(_radiusOne, _radiusTwo, t) : _radiusOne;

        for (int i = 0; i < _sides; i++)
        {
            var x = Mathf.Cos(angle);
            var y = Mathf.Sin(angle);

            circle[i] = _positions[index] + side * x * radius + up * y * radius;

            angle += angleStep;
        }

        return circle;
    }
}