using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Camera _mainCamera;

    private Vector2 _dragStart;
    private Vector2 _dragEnd;
    [SerializeField] private PlayerManager _playerManager;
    private float _moveSpeed;
    private float _rotateSpeed;
    private Enum_RotationMode _rotationMode;

    private bool _isDragging;
    private bool _isLaunched;

    public bool IsLaunched => _isLaunched;

    #region INITIALIZE
    public void Initialize(float moveSpeed, float rotateSpeed, Enum_RotationMode rotationMode, Vector2 startPos)
    {
        Cache();

        _moveSpeed = moveSpeed;
        _rotateSpeed = rotateSpeed;
        _rotationMode = rotationMode;

        _isDragging = false;
        _isLaunched = false;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = _rotationMode == Enum_RotationMode.Constant ? _rotateSpeed : 0f;

        transform.SetPositionAndRotation(startPos, Quaternion.identity);

        if(gameObject.activeSelf == false)
        {
            gameObject.SetActive(true);
        }

        NULLCHECK();
    }

    private void Cache()
    {
        _rb ??= GetComponent<Rigidbody2D>();
        _mainCamera ??= Camera.main;

        NULLCHECK();
    }
    private void NULLCHECK()
    {
        if (_rb == null)
        {
            Debug.LogWarning($"{name} : Rigidbody2D not found");
        }

        if (_mainCamera == null)
        {
            Debug.LogWarning($"{name} : MainCamera not found");
        }

        if (_playerManager == null)
        {
            Debug.LogWarning($"{name} : PlayerManager not found");
        }
    }
    #endregion

    #region UNITY EVENT
    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (!_isLaunched)
            return;

        MaintainSpeed();
        HandleRotation();
    }
    #endregion

    #region INPUT
    private void HandleInput()
    {
        if (_isLaunched) return;

        var mouse = Mouse.current;

        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            _dragStart = GetMouseWorldPosition();
            _isDragging = true;
        }

        if(_isDragging)
        {
            Vector2 direction = _dragStart - GetMouseWorldPosition();

            _playerManager.DrawPredictionLine(transform.position, direction);
        }

        if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
        {

            _dragEnd = GetMouseWorldPosition();

            _playerManager.HidePredictionLine();

            Launch();

            _isDragging = false;
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        return _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
    #endregion

    #region PLAYER
    private void Launch()
    {
        Vector2 direction = _dragStart - _dragEnd;

        if (direction.sqrMagnitude < 0.01f) return;

        _rb.linearVelocity = direction.normalized * _moveSpeed;

        if (_rotationMode == Enum_RotationMode.Constant)
        {
            _rb.angularVelocity = _rotateSpeed;
        }

        _isLaunched = true;
    }

    private void MaintainSpeed()
    {
        if (_rb.linearVelocity.sqrMagnitude <= 0.01f)
            return;

        _rb.linearVelocity = _rb.linearVelocity.normalized * _moveSpeed;
    }

    private void HandleRotation()
    {
        switch (_rotationMode)
        {
            case Enum_RotationMode.Normal:
                break;

            case Enum_RotationMode.Constant:
                _rb.angularVelocity = _rotateSpeed;
                break;

            case Enum_RotationMode.MaxClamp:
                _rb.angularVelocity = Mathf.Clamp(_rb.angularVelocity, -_rotateSpeed, _rotateSpeed);
                break;
        }
    }
    #endregion
}