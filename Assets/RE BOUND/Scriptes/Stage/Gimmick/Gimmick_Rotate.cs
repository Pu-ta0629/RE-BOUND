using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Gimmick_Rotate : MonoBehaviour, IGimmick
{
    [SerializeField] private Enum_RotateType _rotateType;

    [SerializeField] private float _rotateSpeed = 180f;

    private Rigidbody2D _rb;
    private PolygonCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    public void Initialize()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<PolygonCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        NULLCHECK();

        //_rb.constraints = RigidbodyConstraints2D.FreezePosition;

        _collider.autoTiling = true;
        _collider.CreateFromSprite(_spriteRenderer.sprite);
        if (_rotateType == Enum_RotateType.Physics && _rb != null)
        {
            _rb.angularVelocity = _rotateSpeed;
            _rb.mass = 0.0001f;
            _rb.angularDamping = 0;
        }
    }

    public void InitializeGoal(Enum_RotateType rotateType, float rotateSpeed)
    {
        _rotateType = rotateType;
        _rotateSpeed = rotateSpeed;

        if (_rotateType == Enum_RotateType.Physics)
        {
            _rb.angularVelocity = _rotateSpeed;
            _rb.mass = 0.0001f;
            _rb.angularDamping = 0;
        }
    }

    private void NULLCHECK()
    {
        if(_rb == null)
        {
            Debug.LogWarning($"{this.name} : Rigidbody not found");
        }

        if (_collider == null)
        {
            Debug.LogWarning($"{this.name} : PolygonCollider2D not found");
        }

        if (_spriteRenderer == null)
        {
            Debug.LogWarning($"{this.name} : SpriteRenderer not found");
        }
    }

    public void Rotate()
    {
        if (GameController.Instance == null) return;
        if (!GameController.Instance.IsPlaying) return;

        if (_rotateType == Enum_RotateType.Constant)
        {
            transform.Rotate(Vector3.forward, _rotateSpeed * Time.deltaTime);
        }
    }

    public void OnPlayerHit(PlayerMovement player){}

    public void OnPlayerMiss(PlayerMovement player){}

    public void OnActiveEvent()
    {
        _rotateSpeed *= -1f;
    }

    private void Update()
    {
        Rotate();
    }
}
