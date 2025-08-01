using Unity.Cinemachine;
using UnityEngine;

public class BackgroundOffset : MonoBehaviour
{
    private static readonly int CharacterWs = Shader.PropertyToID("CharacterWS");

    // TODO: 用Camera的 bounding 来确定 offset
    private const float _yOffset = 20f;
    private const float _xOffset = 23f;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Transform _character;
    private Material _material;

    [Header("MODIFY")] [SerializeField] private float _xOffsetSpeed = 1f;
    [SerializeField] private float _yOffsetSpeed = 1f;

    [Header("DO NOT MODIFY")] [SerializeField]
    private int[] _triangles;

    [SerializeField] private Vector2[] _uvs;
    [SerializeField] private Vector3[] _vertices;
    private Vector3 _lastFramePos = Vector3.zero;
    private CinemachineBrain _brain;
    private float _height;
    private float _width;

    private void Start()
    {
        InitMaterial();
    }

    private void Update()
    {
        float xOffset = (_character.position.x - _lastFramePos.x) * _xOffsetSpeed;
        float yOffset = (_character.position.y - _lastFramePos.y) * _yOffsetSpeed;

        Vector3 minMin = new Vector3(_character.transform.position.x - _width, _vertices[0].y, 1f);
        Vector3 maxMin = new Vector3(_character.transform.position.x + _width, _vertices[1].y, 1f);
        Vector3 maxMax = new Vector3(_character.transform.position.x + _width, _vertices[2].y, 1f);
        Vector3 minMax = new Vector3(_character.transform.position.x - _width, _vertices[3].y, 1f);

        _vertices = new[] { minMin, maxMin, maxMax, minMax };

        _uvs[0] = new Vector2(_uvs[0].x + xOffset, _uvs[0].y + yOffset);
        _uvs[1] = new Vector2(_uvs[1].x + xOffset, _uvs[1].y + yOffset);
        _uvs[2] = new Vector2(_uvs[2].x + xOffset, _uvs[2].y + yOffset);
        _uvs[3] = new Vector2(_uvs[3].x + xOffset, _uvs[3].y + yOffset);

        _meshFilter.mesh.vertices = _vertices;
        _meshFilter.mesh.uv = _uvs;
        _meshRenderer.sharedMaterial.SetVector(CharacterWs, _character.position);
        _lastFramePos = _character.position;
    }

    private void InitMaterial()
    {
        _brain = Camera.main.GetComponent<CinemachineBrain>();
        Mesh mesh = new Mesh();
        _height = _brain.OutputCamera.orthographicSize + 7f;
        _width = _brain.OutputCamera.aspect * _brain.OutputCamera.orthographicSize;

        Vector3 minMin = new Vector3(_character.transform.position.x - _width,
            _character.transform.position.y - _height / 4, 1f);
        Vector3 maxMin = new Vector3(_character.transform.position.x + _width,
            _character.transform.position.y - _height / 4, 1f);
        Vector3 maxMax = new Vector3(_character.transform.position.x + _width,
            _character.transform.position.y + _height, 1f);
        Vector3 minMax = new Vector3(_character.transform.position.x - _width,
            _character.transform.position.y + _height, 1f);

        _triangles = new[] { 0, 1, 2, 0, 2, 3 };
        _vertices = new[] { minMin, maxMin, maxMax, minMax };
        _uvs = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };

        _meshFilter.mesh = mesh;
        _meshFilter.mesh.MarkDynamic();
        _meshFilter.mesh.SetVertices(_vertices);
        _meshFilter.mesh.SetUVs(0, _uvs);
        _meshFilter.mesh.SetTriangles(_triangles, 0);
    }
}