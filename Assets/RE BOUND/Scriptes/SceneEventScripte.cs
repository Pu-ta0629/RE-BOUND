using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneEventScript", menuName = "Scriptable Objects/SceneEventScript")]
public class SceneEventScript : ScriptableObject
{
    public event Action<StaticSceneAsset> OnSceneEvent;

    public void TriggerEvent(StaticSceneAsset sceneAsset)
    {
        OnSceneEvent?.Invoke(sceneAsset);
    }
}