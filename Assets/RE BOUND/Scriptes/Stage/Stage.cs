using UnityEngine;

public class Stage : MonoBehaviour
{
    public void BeginSceneTransition() =>PlayerManager.Instance.Player.BeginSceneTransition();
    public void EndSceneTransition() => PlayerManager.Instance.Player.EndSceneTransition();
}
