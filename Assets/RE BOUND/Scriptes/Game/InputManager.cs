using UnityEngine.InputSystem;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GimmickManager _gimmickManager;
    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        var keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            _gimmickManager.InvokeActiveEvent();
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            RetryManager.Instance.Retry();
        }
    }
}