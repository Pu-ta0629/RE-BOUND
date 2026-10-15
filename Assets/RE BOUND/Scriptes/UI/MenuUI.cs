using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _menuRoot;

    [Header("State Group")]
    [SerializeField] private GameObject _inGameGroup;       // InGame 用ボタン群
    [SerializeField] private GameObject _stageSelectGroup;  // StageSelect 用ボタン群
    [SerializeField] private GameObject _startGroup;        // Title 用ボタン群

    [Header("Tutorial")]
    [SerializeField] private GameObject _tutorialPanel;

    [Header("Button")]
    [SerializeField] private Button _tutorialButton;
    [SerializeField] private Button _stageSelectButton;
    [SerializeField] private Button _titleButton_InGame;
    [SerializeField] private Button _titleButton_StageSelect;

    [Header("Stage")]
    [SerializeField] private TMP_Text _currentStageText;
    [SerializeField] private StarDisplay _currentStageStars;

    [Header("Audio")]
    [SerializeField] private Toggle _seToggle;
    [SerializeField] private Slider _seSlider;

    [Header("Setting")]
    [SerializeField] private TMP_Dropdown _fpsDropdown;

    private bool _isVisible;
    private StateManager _subscribed;
    public bool IsVisible => _isVisible;

    #region UNITY EVENT

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }

        if (keyboard.fKey.wasPressedThisFrame)
        {
            Debug.Log($"FPS : {(int)GameSettingsManager.Instance.CurrentFPS}");
        }
    }

    private void OnDestroy()
    {
        Unrequest();
    }

    #endregion

    #region INITIALIZE

    //StateManager.Initialize の後
    //Stateによって表示させるMenuが違うため
    public void Initialize()
    {
        Request();

        RegisterButtons();
        RegisterSettings();

        if (_tutorialPanel != null) _tutorialPanel.SetActive(false);

        UpdateStateUI(Enum_GameState.None);
        UpdateStageText();

        Hide();
    }

    private void Request()
    {
        Unrequest();

        if (StateManager.Instance == null)
        {
            Debug.LogError($"{name} : StateManager.Instance is null (StateManager.Initialize を先に呼ぶ)", this);
            return;
        }

        _subscribed = StateManager.Instance;
        _subscribed.OnStateChanged += OnStateChanged;
    }

    private void Unrequest()
    {
        if (_subscribed == null) return;

        _subscribed.OnStateChanged -= OnStateChanged;
        _subscribed = null;
    }

    private void RegisterButtons()
    {
        _tutorialButton.onClick.RemoveAllListeners();
        _tutorialButton.onClick.AddListener(ToggleTutorial);
        _stageSelectButton.onClick.RemoveAllListeners();
        _stageSelectButton.onClick.AddListener(OpenStageSelect);
        _titleButton_InGame.onClick.RemoveAllListeners();
        _titleButton_InGame.onClick.AddListener(OpenTitle);
        _titleButton_StageSelect.onClick.RemoveAllListeners();
        _titleButton_StageSelect.onClick.AddListener(OpenTitle);
    }

    private void RegisterSettings()
    {
        // --- SE ---
        if (_seToggle == null || _seSlider == null)
        {
            if (_seToggle == null) LogMissing(nameof(_seToggle));
            if (_seSlider == null) LogMissing(nameof(_seSlider));
        }
        else
        {
            _seToggle.onValueChanged.RemoveAllListeners();
            _seToggle.onValueChanged.AddListener(SetSE);

            _seSlider.onValueChanged.RemoveAllListeners();
            _seSlider.onValueChanged.AddListener(SetSEVolume);

            AudioManager audioManager = AudioManager.Instance;

            if (audioManager == null)
            {
                Debug.LogError($"{name} : AudioManager.Instance is null (GameManager.Initialize で AudioManager.Initialize() を呼んでいるか確認)");
            }
            else
            {
                // トグル ON = 音が出る（ミュートの逆）
                _seSlider.SetValueWithoutNotify(audioManager.SEVolume);
                _seToggle.SetIsOnWithoutNotify(!audioManager.IsMuted);
                _seSlider.gameObject.SetActive(!audioManager.IsMuted);
            }
        }

        // --- FPS ---
        if (_fpsDropdown == null)
        {
            LogMissing(nameof(_fpsDropdown));
        }
        else
        {
            _fpsDropdown.onValueChanged.RemoveAllListeners();
            _fpsDropdown.onValueChanged.AddListener(SetFPS);
        }
    }

    private void LogMissing(string fieldName)
    {
        Debug.LogError($"{this.name} : {fieldName} not found");
    }

    #endregion

    #region SETTINGS
    private void SetSE(bool value)
    {
        AudioManager.Instance.SetMute(!value);
        _seSlider.gameObject.SetActive(value);
    }

    private void SetSEVolume(float value)
    {
        AudioManager.Instance.SetSEVolume(value);
    }

    private void SetFPS(int value)
    {
        switch (value)
        {
            case 0: GameSettingsManager.Instance.SetFPS(30); break;
            case 1: GameSettingsManager.Instance.SetFPS(60); break;
            case 2: GameSettingsManager.Instance.SetFPS(144); break;
            case 3: GameSettingsManager.Instance.SetFPS(-1); break;
        }
    }
    #endregion

    #region MENU
    public void ToggleMenu()
    {
        if (_isVisible) Hide();
        else Show();

        GameSettingsManager.Instance.TogglePause();
    }

    public void Show()
    {
        _isVisible = true;
        _menuRoot.SetActive(true);
        UpdateStageText();
    }

    public void Hide()
    {
        _isVisible = false;
        _menuRoot.SetActive(false);

        if (StarTooltip.Instance != null)
            StarTooltip.Instance.Hide();
    }
    #endregion

    #region BUTTON
    private void ToggleTutorial()
    {
        _tutorialPanel.SetActive(!_tutorialPanel.activeSelf);
    }

    private void OpenStageSelect()
    {
        Hide();

        GameSettingsManager.Instance.SetPause(false);
        GameManager.Instance.GoStageSelect();
    }

    private void OpenTitle()
    {
        Hide();

        GameSettingsManager.Instance.SetPause(false);
        GameManager.Instance.GoTitle();
    }
    #endregion

    #region STATE
    private void OnStateChanged(Enum_GameState state)
    {
        UpdateStateUI(state);
        UpdateStageText();

        GameSettingsManager.Instance.SetPause(false);
        Hide();
    }

    private void UpdateStateUI(Enum_GameState state)
    {
        _inGameGroup.SetActive(state == Enum_GameState.InGame);
        _stageSelectGroup.SetActive(state == Enum_GameState.StageSelect);
        _startGroup.SetActive(state == Enum_GameState.Title);
    }

    private void UpdateStageText()
    {
        int stageID = GameManager.Instance.CurrentStageID;

        _currentStageText.text = $"STAGE {stageID:00}";

        if (_currentStageStars != null)
        {
            _currentStageStars.Show(GameManager.Instance.SaveManager.GetStars(stageID), stageID);
        }
    }
    #endregion
}
