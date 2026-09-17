using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;
    private Rigidbody2D _rb;
    private Camera _mainCamera;

    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private float _minDragDistance = 2;
    [SerializeField] private ParticleSystem _playerIdle;
    [SerializeField] private GameObject _playerIdleObj;

    [Header("Debug")]
    [SerializeField] private Vector2 _dragStart;
    [SerializeField] private Vector2 _dragEnd;
    private float _moveSpeed;
    private float _rotateSpeed;
    private Enum_RotationMode _rotationMode;

    private bool _isDragging;
    private bool _isLaunched;
    public bool IsLaunched => _isLaunched;

    private bool _isPerformance = false;
    public bool IsPerformance => _isPerformance;

    private int _bounceCount;
    public int BounceCount => _bounceCount;
    #region INITIALIZE
    public void Initialize(float moveSpeed, float rotateSpeed, Enum_RotationMode rotationMode, Vector2 startPos)
    {
        gameObject.SetActive( false );
        if(Instance == null)
        {
            Instance = this;
        }
        Cache();

        _moveSpeed = moveSpeed;
        _rotateSpeed = rotateSpeed;
        _rotationMode = rotationMode;

        _isDragging = false;
        _isLaunched = false;

        _bounceCount = 0;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = _rotationMode == Enum_RotationMode.Constant ? _rotateSpeed : 0f;

        transform.SetPositionAndRotation(startPos, Quaternion.identity);

        if(gameObject.activeSelf == false)
        {
            gameObject.SetActive(true);
        }
        _playerIdleObj.SetActive(true);
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
        if (!_isLaunched) return;

        MaintainSpeed();
        HandleRotation();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _bounceCount++;
        ContactPoint2D contact = collision.GetContact(0);

        EffectManager.Instance.Play(Enum_EffectType.PlayerBounce, contact.point, contact.normal);
    }
    #endregion

    #region INPUT
    private void HandleInput()
    {
        if (_isPerformance) return;
        if (!GameController.Instance.IsPlaying) return;
        if (_isLaunched) return;

        var mouse = Mouse.current;
        if (mouse == null) return;
        if (mouse.leftButton.wasPressedThisFrame)
        {
            _dragStart = GetMouseWorldPosition();
            _isDragging = true;
        }

        if (_isDragging)
        {
            Vector2 direction = _dragStart - GetMouseWorldPosition();

            if (IsValidDrag(direction))
            {
                _playerManager.DrawPredictionLine(transform.position, direction);
            }
            else
            {
                _playerManager.HidePredictionLine();
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
        {

            _dragEnd = GetMouseWorldPosition();

            _playerManager.HidePredictionLine();

            Launch();

            _isDragging = false;
        }

        bool isStartIdleParticle = !_isDragging && !_isLaunched;
        if (isStartIdleParticle)
        {
            if (_playerIdle.isStopped)
            {
                _playerIdle.Play();
                _playerIdleObj.SetActive(true);
            }
        }
        else
        {
            if (_playerIdle.isPlaying)
            {
                _playerIdle.Stop();
                _playerIdleObj.SetActive(false);
            }
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        return _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
    #endregion

    #region PLAYER
    public void SetPerformance(bool value)
    {
        _isPerformance = value;
    }
    private void Launch()
    {
        Vector2 direction = _dragStart - _dragEnd;

        //if (direction.sqrMagnitude < 0.01f) return;
        if (!IsValidDrag(direction)) return;

        _rb.linearVelocity = direction.normalized * _moveSpeed;

        if (_rotationMode == Enum_RotationMode.Constant)
        {
            _rb.angularVelocity = _rotateSpeed;
        }

        _isLaunched = true;
    }

    private bool IsValidDrag(Vector2 direction)
    {
        return direction.sqrMagnitude >= _minDragDistance * _minDragDistance;
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