using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Camera _mainCamera;

    [SerializeField] private PlayerManager _playerManager;

    [Header("Drag")]
    [SerializeField] private float _minDragDistance = 2f;

    [Header("Movement")]
    [SerializeField] private Enum_MovePhysicsMode _movePhysicsMode;
    [SerializeField] private float _deceleration = 0.985f;
    [SerializeField] private float _stopSpeed = 0.2f;

    [Header("Idle")]
    [SerializeField] private ParticleSystem _playerIdle;
    [SerializeField] private GameObject _playerIdleObj;

    [Header("SE")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _se;

    [Header("Debug")]
    [SerializeField] private Vector2 _dragStart;
    [SerializeField] private Vector2 _dragEnd;

    private float _moveSpeed;
    private float _currentSpeed;
    private float _rotateSpeed;

    private Enum_RotationMode _rotationMode;

    private bool _isDragging;
    private bool _isLaunched;
    private bool _isPerformance;

    private int _bounceCount;

    public bool IsLaunched => _isLaunched;
    public bool IsPerformance => _isPerformance;
    public int BounceCount => _bounceCount;

    #region INITIALIZE

    public void Initialize(StageData stageData)
    {
        gameObject.SetActive(false);

        Cache();

        _moveSpeed = stageData.MoveSpeed;
        _rotateSpeed = stageData.RotateSpeed;
        _rotationMode = stageData.RotationMode;

        _currentSpeed = 0f;

        _isDragging = false;
        _isLaunched = false;
        _isPerformance = false;

        _bounceCount = 0;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = _rotationMode == Enum_RotationMode.Constant ? _rotateSpeed : 0f;

        transform.SetPositionAndRotation(stageData.StartPos,Quaternion.identity);
        gameObject.SetActive(true);
        _playerIdleObj.SetActive(true);

        NULLCHECK();
    }

    private void Cache()
    {
        _rb ??= GetComponent<Rigidbody2D>();
        _mainCamera ??= Camera.main;
    }

    private void NULLCHECK()
    {
        if (_rb == null)
            Debug.LogWarning($"{name} : Rigidbody2D not found");

        if (_mainCamera == null)
            Debug.LogWarning($"{name} : MainCamera not found");

        if (_playerManager == null)
            Debug.LogWarning($"{name} : PlayerManager not found");
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

        HandleMove();
        HandleRotation();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _bounceCount++;

        ContactPoint2D contact = collision.GetContact(0);
        EffectManager.Instance.Play(Enum_EffectType.PlayerBounce, contact.point, contact.normal); 

        foreach (AudioClip clip in _se)
        {
            StartCoroutine(PlaySE(clip));
        }
    }

    private IEnumerator PlaySE(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
        //yield return new WaitForSeconds(0.15f);
        yield return null;
    }

    #endregion

    #region INPUT

    private void HandleInput()
    {
        if (_isPerformance) return;
        if (!GameController.Instance.IsPlaying) return;
        if (_isLaunched) return;

        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

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

        bool showIdle = !_isDragging && !_isLaunched;

        if (showIdle)
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

        if (!IsValidDrag(direction))
            return;

        _currentSpeed = _moveSpeed;
        _rb.linearVelocity = direction.normalized * _moveSpeed;

        if (_rotationMode == Enum_RotationMode.Constant)
        {
            _rb.angularVelocity = _rotateSpeed;
        }

        _isLaunched = true;
    }

    private bool IsValidDrag(Vector2 direction)
    {
        return direction.sqrMagnitude >=
               _minDragDistance * _minDragDistance;
    }

    private void HandleMove()
    {
        switch (_movePhysicsMode)
        {
            case Enum_MovePhysicsMode.Constant:
                MaintainSpeed();
                break;

            case Enum_MovePhysicsMode.Physics:
                MonsterMove();
                break;
        }
    }

    private void MaintainSpeed()
    {
        if (_rb.linearVelocity.sqrMagnitude <= 0.0001f) return;
        _rb.linearVelocity = _rb.linearVelocity.normalized * _moveSpeed;
    }

    private void MonsterMove()
    {
        _currentSpeed *= Mathf.Pow(_deceleration, Time.fixedDeltaTime * 60f);

        if (_currentSpeed <= _stopSpeed)
        {
            StopPlayer();
            return;
        }

        if (_rb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * _currentSpeed;
        }
    }
    private void StopPlayer()
    {
        _currentSpeed = 0f;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        _isLaunched = false;
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