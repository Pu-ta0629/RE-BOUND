using UnityEngine;

public class Gimmick_Goal : MonoBehaviour, IGimmick
{

    private PolygonCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    public void Initialize()
    {
        Debug.Log(StageManager.Instance);
        if (StageManager.Instance != null)
        {
            Debug.Log(StageManager.Instance.CurrentStageData);
        }
        //transform.position = StageManager.Instance.CurrentStageData.GoalPos;
        transform.SetPositionAndRotation(StageManager.Instance.CurrentStageData.GoalPos, Quaternion.identity);

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
    public void OnPlayerHit(PlayerMovement player)
    {
        GameController.Instance.NextStage();
    }
    public void OnPlayerMiss(PlayerMovement player){}
    public void OnActiveEvent(){}
}