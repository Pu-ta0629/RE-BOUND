using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WallHitEffect : MonoBehaviour
{
    private Material materialInstance;
    private SpriteRenderer _spriteRenderer;
    private float radius;
    private bool playing;

    [SerializeField] private float speed = 10f;

    [SerializeField] private float maxRadius = 1f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        materialInstance = Instantiate(_spriteRenderer.material);

        _spriteRenderer.material = materialInstance;

        materialInstance.SetTexture("_WallMask", _spriteRenderer.sprite.texture);
    }

    private void Update()
    {
        if (!playing) return;

        radius += Time.deltaTime * speed;
        materialInstance.SetFloat("_Radius", radius);

        if (radius >= maxRadius)
        {
            playing = false;

            materialInstance.SetFloat("_Radius", -100f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.gameObject != PlayerManager.Instance.Player) return;
        //if (!collision.CompareTag("Player")) return;

        ContactPoint2D contact = collision.GetContact(0);

        //materialInstance.SetVector("_HitPosition", contact.point);

        //radius = 0f;

        //materialInstance.SetFloat("_Radius", radius);

        playing = true;

        EffectManager.Instance.PlayWallBounce(Enum_EffectType.WallBounce, contact.point, contact.normal);
    }
}