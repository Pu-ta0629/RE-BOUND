using System.Collections;
using TMPro;
using UnityEngine;
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
    [SerializeField] private Button _closeButton_Menu;

    [Header("Transition (遷移アニメ。Animatorは使わず、コードで再生)")]
    [Tooltip("InGame 側のクリップのパスの基準（元の Animator が付いていた InGameUIRoot など）")]
    [SerializeField] private GameObject _inGameTarget;
    [Tooltip("StageSelect 側のクリップのパスの基準（StageSelectRoot）")]
    [SerializeField] private GameObject _stageSelectTarget;

    [Tooltip("InGame の Button_StageSelect を押したとき。空ならアニメなしで遷移")]
    [SerializeField] private AnimationClip _inGameToStageSelectClip;
    [Tooltip("InGame の Button_Title を押したとき")]
    [SerializeField] private AnimationClip _inGameToTitleClip;
    [Tooltip("StageSelect の Button_Title を押したとき")]
    [SerializeField] private AnimationClip _stageSelectToTitleClip;

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
    private bool _isBusy;
    private readonly ViewSnapshot _menuSnapshot = new();
    public bool IsVisible => _isVisible;
    public bool IsBusy => _isBusy;

    #region UNITY EVENT

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

        // 遷移アニメで動かした見た目を、閉じるたびに元に戻すため（ON/OFFは State が管理するので戻さない）
        _menuSnapshot.Clear();
        _menuSnapshot.Add(_menuRoot.transform, false);

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
        _tutorialButton.onClick.AddListener(GameManager.Instance.ButtonSE.ButtonClick);
        _tutorialButton.onClick.AddListener(ToggleTutorial);

        _stageSelectButton.onClick.RemoveAllListeners();
        _stageSelectButton.onClick.AddListener(GameManager.Instance.ButtonSE.ButtonClick);
        _stageSelectButton.onClick.AddListener(() => Transition(_inGameTarget, _inGameToStageSelectClip, GameManager.Instance.GoStageSelect));

        _titleButton_InGame.onClick.RemoveAllListeners();
        _titleButton_InGame.onClick.AddListener(GameManager.Instance.ButtonSE.ButtonClick);
        _titleButton_InGame.onClick.AddListener(() => Transition(_inGameTarget, _inGameToTitleClip, GameManager.Instance.GoTitle));

        _titleButton_StageSelect.onClick.RemoveAllListeners();
        _titleButton_StageSelect.onClick.AddListener(GameManager.Instance.ButtonSE.ButtonClick);
        _titleButton_StageSelect.onClick.AddListener(() => Transition(_stageSelectTarget, _stageSelectToTitleClip, GameManager.Instance.GoTitle));

        _closeButton_Menu.onClick.RemoveAllListeners();
        _closeButton_Menu.onClick.AddListener(GameManager.Instance.ButtonSE.ButtonClick);
        _closeButton_Menu.onClick.AddListener(ToggleMenu);
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
        if (_isBusy) return;

        if (_isVisible) Hide();
        else Show();

        // 止めるのは InGame だけ。Title / StageSelect はメニューを開いても時間は進める
        bool inGame = StateManager.Instance.CurrentState == Enum_GameState.InGame;
        GameSettingsManager.Instance.SetPause(_isVisible && inGame);
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
        _menuSnapshot.Restore();

        if (StarTooltip.Instance != null)
            StarTooltip.Instance.Hide();
    }
    #endregion

    #region BUTTON
    private void ToggleTutorial()
    {
        _tutorialPanel.SetActive(!_tutorialPanel.activeSelf);
    }

    // 遷移アニメを再生してから State を切り替える
    private void Transition(GameObject target, AnimationClip clip, System.Action go)
    {
        if (_isBusy) return;

        _isBusy = true;

        // メニューは開いたまま、メニューのボタンなどのアニメを見せる（閉じるのは State が切り替わるとき）
        // MenuUI が非表示になっても止まらないように GameManager で動かす
        GameManager.Instance.StartCoroutine(TransitionRoutine(target, clip, go));
    }

    private IEnumerator TransitionRoutine(GameObject target, AnimationClip clip, System.Action go)
    {
        // ポーズ中でも進む（unscaled）。ポーズは State が切り替わるときに解除される
        yield return ClipPlayer.Play(target, clip);

        go();
    }
    #endregion

    #region STATE
    private void OnStateChanged(Enum_GameState state)
    {
        _isBusy = false;

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