using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WallHitEffect : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject != PlayerManager.Instance.Player.gameObject) return;

        ContactPoint2D contact = collision.GetContact(0);

        //EffectManager.Instance.Play(Enum_EffectType.WallBounce, contact.point, contact.normal);
        EffectManager.Instance.PlayWallBounce(Enum_EffectType.WallBounce, contact.point, contact.normal);
    }
}