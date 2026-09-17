using UnityEngine;

[ExecuteAlways]
public class BoundaryWallController : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;

    [SerializeField] private Transform _wallLeft;
    [SerializeField] private Transform _wallRight;
    [SerializeField] private Transform _wallTop;
    [SerializeField] private Transform _wallBottom;

    [SerializeField] private float _wallThickness = 1f;

    private float _lastSize;
    private float _lastAspect;
    private Vector3 _lastCameraPos;

    private void Start()
    {
        UpdateWalls();
    }
    private void OnValidate()
    {
        if (_mainCamera == null) return;

        UpdateWalls();
    }


    private void Update()
    {
        if (_mainCamera == null) return;

        bool needUpdate = false;
        if (!Mathf.Approximately(_lastSize, _mainCamera.orthographicSize)) needUpdate = true;
        if (!Mathf.Approximately(_lastAspect, _mainCamera.aspect)) needUpdate = true;
        if (_lastCameraPos != _mainCamera.transform.position) needUpdate = true;

        if (needUpdate) UpdateWalls();
    }

    private void UpdateWalls()
    {
        _lastSize = _mainCamera.orthographicSize;
        _lastAspect = _mainCamera.aspect;
        _lastCameraPos = _mainCamera.transform.position;

        float height = _mainCamera.orthographicSize * 2f;
        float width = height * _mainCamera.aspect;

        Vector3 cameraPos = _mainCamera.transform.position;
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        // Left
        _wallLeft.position = new Vector3(cameraPos.x - halfWidth - _wallThickness * 0.5f, cameraPos.y, 0f);
        _wallLeft.localScale = new Vector3(_wallThickness, height + _wallThickness * 2f, 1f);

        // Right
        _wallRight.position = new Vector3(cameraPos.x + halfWidth + _wallThickness * 0.5f, cameraPos.y, 0f);
        _wallRight.localScale = new Vector3(_wallThickness, height + _wallThickness * 2f, 1f);

        // Top
        _wallTop.position = new Vector3(cameraPos.x, cameraPos.y + halfHeight + _wallThickness * 0.5f, 0f);
        _wallTop.localScale = new Vector3(width + _wallThickness * 2f, _wallThickness, 1f);

        // Bottom
        _wallBottom.position = new Vector3(cameraPos.x, cameraPos.y - halfHeight - _wallThickness * 0.5f, 0f);
        _wallBottom.localScale = new Vector3(width + _wallThickness * 2f, _wallThickness, 1f);
    }
}