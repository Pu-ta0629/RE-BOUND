using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class Gimmick_Retry : MonoBehaviour, IGimmick
{
    private PolygonCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    public void Initialize()
    {
        _collider = GetComponent<PolygonCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        NULLCHECK();

        _collider.isTrigger = true;
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
        GameController.Instance.Retry();
    }

    public void OnPlayerMiss(PlayerMovement player){}

    public void OnActiveEvent(){}
}