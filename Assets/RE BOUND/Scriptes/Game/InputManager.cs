using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GimmickManager _gimmickManager;
    [SerializeField] private MenuUI _menuUI;
    [SerializeField] private RadialMenu _radialMenu;

    private bool _isInitialized;

    public void Initialize()
    {
        if (_gimmickManager == null) Debug.LogWarning($"{name} : GimmickManager not found");
        if (_menuUI == null) Debug.LogWarning($"{name} : MenuUI not found");
        if (_radialMenu == null) Debug.LogWarning($"{name} : RadialMenu not found");

        if (_radialMenu != null) _radialMenu.Initialize();

        _isInitialized = true;
    }

    private void Update()
    {
        if (!_isInitialized) return;

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        // メニューは全Stateで開ける（表示内容は MenuUI が State ごとに切り替える）
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            _menuUI.ToggleMenu();
        }

        // ゲーム操作は InGame のみ。メニュー表示中も受け付けない
        bool canPlay = StateManager.Instance.CurrentState == Enum_GameState.InGame
                       && !_menuUI.IsVisible && !_menuUI.IsBusy;

        if (!canPlay)
        {
            if (_radialMenu != null && _radialMenu.IsOpen) _radialMenu.Close();
            return;
        }

        if (keyboard != null)
        {
            if (keyboard.spaceKey.wasPressedThisFrame) _gimmickManager.InvokeActiveEvent();
            if (keyboard.rKey.wasPressedThisFrame) Retry();
        }

        if (mouse != null && _radialMenu != null) UpdateRadialMenu(mouse);
    }

    // 右クリックを押す -> 表示 / 押している間 -> 項目を選ぶ / 離す -> 選んだ項目を実行して非表示
    private void UpdateRadialMenu(Mouse mouse)
    {
        Vector2 mousePosition = mouse.position.ReadValue();

        if (mouse.rightButton.wasPressedThisFrame) _radialMenu.Open(mousePosition);

        if (!_radialMenu.IsOpen) return;

        Enum_RadialItem selected = _radialMenu.Select(mousePosition);

        if (!mouse.rightButton.wasReleasedThisFrame) return;

        _radialMenu.Close();

        switch (selected)
        {
            case Enum_RadialItem.Retry: Retry(); break;
            case Enum_RadialItem.Menu: _menuUI.ToggleMenu(); break;
                // Cancel(×) と None(どれにも重なっていない)は何もしない
        }
    }

    private void Retry()
    {
        GameManager.Instance.ReloadStage();
    }
}