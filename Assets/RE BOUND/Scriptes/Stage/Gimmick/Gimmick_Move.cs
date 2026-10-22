using UnityEngine;

/// <summary>
/// 動くギミック。
///   Horizontal / Vertical : 開始位置から MoveRange 離れたら折り返す（元の処理）
///   Free                  : 開始位置を中心に、半径 MoveRange の円の中をランダムに動き回る
/// </summary>
public class Gimmick_Move : MonoBehaviour, IGimmick
{
    [SerializeField] private Enum_MoveType _moveType;
    [SerializeField] private float _moveSpeed = 3f;
    [Tooltip("Horizontal / Vertical : 開始位置からの距離 / Free : 動き回る円の半径")]
    [SerializeField] private float _moveRange = 5f;

    [Tooltip("Free のときの曲がる速さ（度/秒）。小さいほど大きく弧を描く")]
    [SerializeField] private float _freeTurnRate = 180f;
    [Tooltip("Free のとき、目的地に着いたとみなす距離")]
    [SerializeField] private float _freeArriveDistance = 0.3f;
    [Tooltip("Free のとき、目的地に着けなくても次の目的地に変える秒数")]
    [SerializeField] private float _freeRetargetTime = 4f;

    // 旧: Free の方向指定。現在は使わない（Gimmick_Goal の呼び出し互換のために残している）
    [HideInInspector, SerializeField] private Vector2 _freeDirection;

    private Vector2 _startPosition;

    private int _direction = 1;

    // Free
    private Vector2 _heading = Vector2.right;
    private Vector2 _target;
    private float _targetTimer;

    private PolygonCollider2D _collider;
    private SpriteRenderer _spriteRenderer;

    public void Initialize()
    {
        _startPosition = transform.position;
        _direction = 1;

        ResetFree();

        _collider = GetComponent<PolygonCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        NULLCHECK();

        _collider.autoTiling = true;
        _collider.CreateFromSprite(_spriteRenderer.sprite);
    }

    public void InitializeGoal(Enum_MoveType moveType, float moveSpeed, float moveRange, Vector2 freeDirection)
    {
        _moveType = moveType;
        _moveSpeed = moveSpeed;
        _moveRange = moveRange;
        _freeDirection = freeDirection;

        _startPosition = transform.position;
        _direction = 1;

        ResetFree();
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

        if (_moveType == Enum_MoveType.Free)
        {
            MoveFree();
            return;
        }

        // Horizontal / Vertical（元の処理）
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
            _ => Vector2.right
        };
    }

    #region FREE

    private void ResetFree()
    {
        _heading = Random.insideUnitCircle.normalized;
        if (_heading == Vector2.zero) _heading = Vector2.right;

        PickTarget();
    }

    // 円の中のランダムな目的地へ、なめらかに曲がりながら向かう
    private void MoveFree()
    {
        float dt = Time.deltaTime;

        _targetTimer -= dt;

        Vector2 position = transform.position;
        Vector2 toTarget = _target - position;

        // 着いた / 着けないまま時間が経った -> 次の目的地へ
        if (toTarget.magnitude <= _freeArriveDistance || _targetTimer <= 0f)
        {
            PickTarget();
            toTarget = _target - position;
        }

        // 向きを少しずつ目的地へ向ける（急に曲がらない）
        float maxRadians = _freeTurnRate * Mathf.Deg2Rad * dt;
        Vector3 turned = Vector3.RotateTowards(_heading, toTarget.normalized, maxRadians, 0f);

        _heading = ((Vector2)turned).normalized;
        position += _heading * (_moveSpeed * dt);

        // 範囲の外に出ないようにする
        Vector2 fromStart = position - _startPosition;

        if (fromStart.magnitude > _moveRange)
        {
            position = _startPosition + fromStart.normalized * _moveRange;
        }

        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    private void PickTarget()
    {
        // 縁ぎりぎりを避けて、範囲の内側から選ぶ
        _target = _startPosition + Random.insideUnitCircle * (_moveRange * 0.9f);
        _targetTimer = _freeRetargetTime;
    }

    #endregion

    public void OnPlayerHit(PlayerMovement player) { }

    public void OnPlayerMiss(PlayerMovement player) { }

    public void OnActiveEvent()
    {
        if (_moveType == Enum_MoveType.Free)
        {
            // 向きを反転して、新しい目的地を選ぶ
            _heading = -_heading;
            PickTarget();
        }
        else
        {
            _direction *= -1;
        }
    }

    private void Update()
    {
        Move();
    }
}