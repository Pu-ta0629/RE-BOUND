using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 全State共通の入力受付。どのStateの子にも置かず、常に有効なオブジェクトに付ける。
/// Awake / Start は持たず、GameManager から Initialize() で初期化される。
/// </summary>
public class InputManager : MonoBehaviour
{
    [SerializeField] private GimmickManager _gimmickManager;
    [SerializeField] private MenuUI _menuUI;

    private bool _isInitialized;

    public void Initialize()
    {
        if (_gimmickManager == null) Debug.LogWarning($"{name} : GimmickManager not found");
        if (_menuUI == null) Debug.LogWarning($"{name} : MenuUI not found");

        _isInitialized = true;
    }

    private void Update()
    {
        if (!_isInitialized) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // メニューは全Stateで開ける（表示内容は MenuUI が State ごとに切り替える）
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            _menuUI.ToggleMenu();
            AudioManager.Instance.Play(Enum_SEType.ButtonClick);
        }

        // ゲーム操作は InGame のみ。メニュー表示中も受け付けない
        if (StateManager.Instance.CurrentState != Enum_GameState.InGame) return;
        if (_menuUI.IsVisible || _menuUI.IsBusy) return;

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            _gimmickManager.InvokeActiveEvent();
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            AudioManager.Instance.Play(Enum_SEType.Retry);
            RetryManager.Instance.Retry();
        }
    }
}