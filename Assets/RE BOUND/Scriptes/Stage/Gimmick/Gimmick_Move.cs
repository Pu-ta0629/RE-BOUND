using UnityEngine;

public class Gimmick_Move : MonoBehaviour, IGimmick
{
    [SerializeField] private Enum_MoveType _moveType;

    [SerializeField] private float _moveSpeed = 3f;

    [SerializeField] private float _moveRange = 5f;

    [SerializeField] private Vector2 _freeDirection;

    private Vector2 _startPosition;

    private int _direction = 1;

    private PolygonCollider2D _collider;
    private SpriteRenderer _spriteRenderer;

    public void Initialize()
    {
        _startPosition = transform.position;

        _collider = GetComponent<PolygonCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        NULLCHECK();

        _collider.autoTiling = true;
        _collider.CreateFromSprite(_spriteRenderer.sprite);
    }
    private void NULLCHECK()
    {
        if (_collider == null)
        {
            Debug.LogWarning($"{this.name} : PolygonCollider2D not found");
        }

        if (_spriteRenderer == null)
        {
            Debug.LogWarning($"{this.name} : SpriteRenderer not found");
        }
    }
    public void Move()
    {
        if (GameController.Instance == null) return;
        if (!GameController.Instance.IsPlaying) return;

        Vector2 moveDir = GetDirection();

        transform.position += (Vector3)(moveDir * _moveSpeed * _direction * Time.deltaTime);

        float distance = Vector2.Distance(transform.position, _startPosition);

        if (distance >= _moveRange)
        {
            _direction *= -1;
        }
    }

    private Vector2 GetDirection()
    {
        return _moveType switch
        {
            Enum_MoveType.Horizontal => Vector2.right,
            Enum_MoveType.Vertical => Vector2.up,
            Enum_MoveType.Free => _freeDirection.normalized,
            _ => Vector2.right
        };
    }

    public void OnPlayerHit(PlayerMovement player){}

    public void OnPlayerMiss(PlayerMovement player){}

    public void OnActiveEvent()
    {
        _direction *= -1;
    }

    private void Update()
    {
        Move();
    }
}