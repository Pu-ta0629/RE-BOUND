using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "SceneAsset", menuName = "Scriptable Objects/SceneAsset")]
public class StaticSceneAsset : ScriptableObject
{
    [SerializeField] protected string value;
    public string Value => value;

#if UNITY_EDITOR
    [SerializeField] protected SceneAsset sceneAsset;
    public void OnValidate()
    {
        if (sceneAsset == null) return;
        value = sceneAsset.name;
    }
#endif
}

