using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [System.Serializable]
    public class PlayerData
    {
        public Enum_PlayerType PlayerType;
        public Sprite Sprite;
    }

    [Header("Player")]
    [SerializeField] private PlayerMovement _playerMovement;
    public PlayerMovement Player => _playerMovement;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private PolygonCollider2D _polygonCollider;

    [Header("Prediction")]
    [SerializeField] private LineRenderer _lineRenderer;

    [SerializeField] private int _maxReflection = 2;
    [Tooltip("予測線の全長（反射を含む）。ドラッグが最小距離のとき")]
    [SerializeField] private float _minLineLength = 3f;
    [Tooltip("予測線の全長（反射を含む）。ドラッグが最大距離以上のとき")]
    [SerializeField] private float _maxLineLength = 15f;

    [Header("Sprite")]
    [SerializeField] private List<PlayerData> _playerDataList;
    private Enum_PlayerType _currentPlayerType;
    public Enum_PlayerType PlayerType => _currentPlayerType;

    [Header("Free Move (Title / StageSelect)")]
    [Tooltip("Title / StageSelect で使う StageData（開始位置・速度・見た目）")]
    [SerializeField] private StageData _freeStageData;

    [Header("Appear (その場で湧いて出る)")]
    [SerializeField] private float _appearDuration = 0.6f;
    [Tooltip("大きさが行き過ぎる量（0でオーバーシュートなし）")]
    [SerializeField, Range(0f, 3f)] private float _appearOvershoot = 1.7f;
    [Tooltip("出現中に回る角度（0で回転なし）")]
    [SerializeField] private float _appearSpin = 180f;

    private Coroutine _appearRoutine;
    private bool _isAppeared;

    public void Initialize(StageData stageData)
    {
        NULLCHECK();

        if (Instance == null)
        {
            Instance = this;
        }

        ChangeSprite(stageData.PlayerType);

        _playerMovement.Initialize(stageData);

        HidePredictionLine();
        _playerMovement.TrailRendererClear();
    }

    // TitleState / StageSelectState に入るたびに呼ばれる。
    // 画面のアニメ(waitWhile が true の間)が終わるまで Player は出さず、終わったら出現アニメ→操作可能にする
    public void BeginFree(Func<bool> waitWhile = null)
    {
        if (_freeStageData == null)
        {
            Debug.LogWarning($"{name} : Free Stage Data not found");
            return;
        }

        Initialize(_freeStageData);
        Hide();
        Appear(waitWhile);
    }

    // Player を隠して操作できない状態にする（InGame に入るときもこれ）
    public void Hide()
    {
        if (_appearRoutine != null) StopCoroutine(_appearRoutine);
        _appearRoutine = null;

        _isAppeared = false;
        _spriteRenderer.enabled = false;
        _playerMovement.BeginSceneTransition();
    }

    // 出現アニメを流して、終わったら操作可能にする。すでに出ている/再生中なら何もしない
    public void Appear(Func<bool> waitWhile = null)
    {
        if (_isAppeared || _appearRoutine != null) return;

        _appearRoutine = StartCoroutine(AppearRoutine(waitWhile));
    }

    private IEnumerator AppearRoutine(Func<bool> waitWhile)
    {
        // 他のアニメが終わるまで待つ
        while (waitWhile != null && waitWhile()) yield return null;

        Transform player = _playerMovement.transform;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        Vector3 baseScale = player.localScale;
        float baseAngle = player.eulerAngles.z;

        // 動かしている間は物理を止める
        rb.simulated = false;

        player.localScale = Vector3.zero;
        _spriteRenderer.enabled = true;

        float time = 0f;

        while (time < _appearDuration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / _appearDuration);

            // 0 → 少し大きくなりすぎる → 1 に落ち着く（EaseOutBack）
            float c1 = _appearOvershoot;
            float c3 = c1 + 1f;
            float u = t - 1f;
            float scale = 1f + c3 * u * u * u + c1 * u * u;

            player.localScale = baseScale * Mathf.Max(0f, scale);

            // 回りながら湧いて、最後に元の角度で止まる
            player.rotation = Quaternion.Euler(0f, 0f, baseAngle + _appearSpin * (1f - t));

            yield return null;
        }

        player.localScale = baseScale;
        player.rotation = Quaternion.Euler(0f, 0f, baseAngle);
        rb.simulated = true;

        _isAppeared = true;
        _appearRoutine = null;
        _playerMovement.EndSceneTransition();
    }

    // dragRate: 0 = 最小距離 / 1 = 最大距離以上。予測線の長さに反映される
    public void DrawPredictionLine(Vector2 startPosition, Vector2 direction, float dragRate)
    {
        List<Vector3> points = new();

        points.Add(startPosition);

        Vector2 currentPos = startPosition;
        Vector2 currentDir = direction.normalized;

        // 全長のうち、まだ伸ばせる残りの長さ
        float remaining = Mathf.Lerp(_minLineLength, _maxLineLength, dragRate);

        for (int i = 0; i <= _maxReflection; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, remaining);

            if (!hit.collider)
            {
                points.Add(currentPos + currentDir * remaining);
                break;
            }

            points.Add(hit.point);

            remaining -= hit.distance;
            if (remaining <= 0.01f || i == _maxReflection) break;

            currentDir = Vector2.Reflect(currentDir, hit.normal);

            currentPos = hit.point + currentDir * 0.01f;
        }

        _lineRenderer.positionCount = points.Count;
        _lineRenderer.SetPositions(points.ToArray());
    }

    public void HidePredictionLine()
    {
        _lineRenderer.positionCount = 0;
    }

    private void ChangeSprite(Enum_PlayerType playerType)
    {
        foreach (PlayerData data in _playerDataList)
        {
            if (data.PlayerType != playerType) continue;

            _currentPlayerType = playerType;
            _spriteRenderer.sprite = data.Sprite;

            _polygonCollider.CreateFromSprite(data.Sprite);

            return;
        }

        Debug.LogWarning($"PlayerType : {playerType} Not Found");
    }

    private void NULLCHECK()
    {
        if (_playerMovement == null) Debug.LogWarning($"{name} : PlayerMovement not found");
        if (_spriteRenderer == null) Debug.LogWarning($"{name} : SpriteRenderer not found");
        if (_polygonCollider == null) Debug.LogWarning($"{name} : PolygonCollider not found");
        if (_lineRenderer == null) Debug.LogWarning($"{name} : LineRenderer not found");
    }
}