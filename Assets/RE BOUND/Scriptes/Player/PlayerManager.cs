using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public Enum_PlayerType PlayerType;
        public Sprite Sprite;
    }

    [Header("Player")]
    [SerializeField] private PlayerMovement _playerMovement;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private PolygonCollider2D _polygonCollider;

    [Header("Prediction")]
    [SerializeField] private LineRenderer _lineRenderer;

    private int _maxReflection = 2;

    [Header("Sprite")]
    [SerializeField] private List<PlayerData> _playerDataList;

    public PlayerMovement SpawnPlayer(StageData stageData)
    {
        ChangeSprite(stageData.PlayerType);

        _playerMovement.Initialize(
            stageData.MoveSpeed,
            stageData.RotateSpeed,
            stageData.RotationMode,
            stageData.StartPos);

        HidePredictionLine();

        return _playerMovement;
    }

    public void Initialize(StageData stageData)
    {
        NULLCHECK();

        ChangeSprite(stageData.PlayerType);

        _playerMovement.Initialize(
            stageData.MoveSpeed,
            stageData.RotateSpeed,
            stageData.RotationMode,
            stageData.StartPos);

        HidePredictionLine();
    }

    public void DrawPredictionLine(Vector2 startPosition, Vector2 direction)
    {
        List<Vector3> points = new();

        points.Add(startPosition);

        Vector2 currentPos = startPosition;
        Vector2 currentDir = direction.normalized;

        for (int i = 0; i < _maxReflection; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir,100f);

            if (!hit.collider)
            {
                points.Add(currentPos + currentDir * 100f);
                break;
            }

            points.Add(hit.point);

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

            _spriteRenderer.sprite = data.Sprite;

            _polygonCollider.CreateFromSprite(data.Sprite);

            return;
        }

        Debug.LogWarning($"PlayerType : {playerType} Not Found");
    }

    private void NULLCHECK()
    {
        if(_playerMovement == null)
        {
            Debug.Log($"{this.name} : PlayerMovement not found");
        }
        if (_spriteRenderer == null)
        {
            Debug.Log($"{this.name} : SpriteRenderer not found");
        }
        if (_polygonCollider == null)
        {
            Debug.Log($"{this.name} : PolygonCollider not found");
        }
        if (_lineRenderer == null)
        {
            Debug.Log($"{this.name} : LineRenderer not found");
        }
    }
}