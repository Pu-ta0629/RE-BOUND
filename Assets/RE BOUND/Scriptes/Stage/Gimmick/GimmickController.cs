using UnityEngine;

//[System.Serializable]
[RequireComponent(typeof(PolygonCollider2D))]
public class GimmickController : MonoBehaviour
{
    private IGimmick _gimmick;

    public void Initialize()
    {
        Cache();

        _gimmick?.Initialize();
    }

    private void Cache()
    {
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();

        foreach (var component in components)
        {
            if (component is IGimmick gimmick)
            {
                _gimmick = gimmick;
                Debug.Log(_gimmick);
                break;
            }
        }

        if (_gimmick == null)
        {
            Debug.LogError($"{name} : IGimmick doesn't exist");
        }
    }

    public void PlayerHit(PlayerMovement player)
    {
        _gimmick?.OnPlayerHit(player);
        Debug.Log("Playre HIT!!");
    }

    public void ActiveEvent()
    {
        _gimmick?.OnActiveEvent();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement player = collision.GetComponent<PlayerMovement>();

        if (player == null) return;

        PlayerHit(player);
    }
}