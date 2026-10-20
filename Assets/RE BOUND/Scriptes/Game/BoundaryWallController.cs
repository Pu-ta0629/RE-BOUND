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
        if (_mainCamera != null) UpdateWalls();
    }

    private void OnValidate()
    {
        if (_mainCamera == null) return;

        UpdateWalls();
    }

    private void Update()
    {
        if (_mainCamera == null) return;

        bool needUpdate = !Mathf.Approximately(_lastSize, _mainCamera.orthographicSize)
                       || !Mathf.Approximately(_lastAspect, _mainCamera.aspect)
                       || Vector3.SqrMagnitude(_lastCameraPos - _mainCamera.transform.position) > 0.0001f;

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

        //left
        _wallLeft.position = new Vector3(cameraPos.x - halfWidth - _wallThickness * 0.5f, cameraPos.y, 0f);
        _wallLeft.localScale = new Vector3(_wallThickness, height + _wallThickness * 2f, 1f);
        //right
        _wallRight.position = new Vector3(cameraPos.x + halfWidth + _wallThickness * 0.5f, cameraPos.y, 0f);
        _wallRight.localScale = new Vector3(_wallThickness, height + _wallThickness * 2f, 1f);
        //top
        _wallTop.position = new Vector3(cameraPos.x, cameraPos.y + halfHeight + _wallThickness * 0.5f, 0f);
        _wallTop.localScale = new Vector3(width + _wallThickness * 2f, _wallThickness, 1f);
        //bottom
        _wallBottom.position = new Vector3(cameraPos.x, cameraPos.y - halfHeight - _wallThickness * 0.5f, 0f);
        _wallBottom.localScale = new Vector3(width + _wallThickness * 2f, _wallThickness, 1f);
    }
}