using UnityEngine;

[RequireComponent(typeof(Gimmick_Move))]
[RequireComponent(typeof(Gimmick_Rotate))]
public class Gimmick_Goal : MonoBehaviour, IGimmick
{

    private PolygonCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private Gimmick_Move _move;
    private Gimmick_Rotate _rotate;

    public void Initialize()
    {
        StageData stageData = StageManager.Instance.CurrentStageData;
        Debug.Log(StageManager.Instance);
        if (StageManager.Instance != null)
        {
            Debug.Log(StageManager.Instance.CurrentStageData);
        }
        transform.SetPositionAndRotation(StageManager.Instance.CurrentStageData.GoalPos, Quaternion.identity);

        _collider = GetComponent<PolygonCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _move = GetComponent<Gimmick_Move>();
        _rotate = GetComponent<Gimmick_Rotate>();

        NULLCHECK();

        _collider.autoTiling = true;
        _collider.CreateFromSprite(_spriteRenderer.sprite);

        _move.InitializeGoal(stageData.GoalMoveType, stageData.GoalMoveSpeed, stageData.GoalMoveRange, stageData.GoalFreeDirection);
        _rotate.InitializeGoal(stageData.GoalRotateType, stageData.GoalRotateSpeed);
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
        if(_move == null)
        {
            Debug.LogWarning($"{this.name} : Gimmick_Move not found");
        }
        if(_rotate == null)
        {
            Debug.LogWarning($"{this.name} : Gimmick_Rotate not found");
        }
    }
    public void OnPlayerHit(PlayerMovement player)
    {
        GameController.Instance.NextStage();
    }
    public void OnPlayerMiss(PlayerMovement player){}
    public void OnActiveEvent(){}
}