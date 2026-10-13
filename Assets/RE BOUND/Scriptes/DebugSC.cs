#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugSC : MonoBehaviour
{
    [Header("OnGUIPreview")]
    [SerializeField] private bool isOnGUIView = true;
    [Header("ref")]
    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private PlayerMovement _playerMovement;

    private void OnGUI()
    {
        if (isOnGUIView)
        {
            GUIStyle customStyle = new GUIStyle(GUI.skin.label);
            customStyle.fontSize = 30;

            GUILayout.BeginArea(new Rect(10, 10, 350, 100));
            GUILayout.Label($"{(_playerMovement.DragStart - GetScreenMouseppPosition()).magnitude}", customStyle);
            GUILayout.EndArea();
        }
    }

    private Vector2 GetScreenMouseppPosition()
    {
        return GameManager.Instance.MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
}

#endif