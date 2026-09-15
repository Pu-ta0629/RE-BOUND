using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86;
using static UnityEngine.Audio.GeneratorInstance;

public class MenuUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _menuRoot;

    [Header("Scene Group")]
    [SerializeField] private GameObject _inGameGroup;
    [SerializeField] private GameObject _stageSelectGroup;
    [SerializeField] private GameObject _startGroup;

    [Header("Tutorial")]
    [SerializeField] private GameObject _tutorialPanel;

    [Header("Button")]
    [SerializeField] private Button _tutorialButton;
    [SerializeField] private Button _stageSelectButton;
    [SerializeField] private Button _titleButton_InGame;
    [SerializeField] private Button _titleButton_StageSelect;

    [Header("Stage")]
    [SerializeField] private TMP_Text _currentStageText;

    [Header("Audio")]
    [SerializeField] private Toggle _bgmToggle;
    [SerializeField] private Toggle _seToggle;

    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _seSlider;

    [Header("Setting")]
    //[SerializeField] private Toggle _vSyncToggle;
    [SerializeField] private TMP_Dropdown _fpsDropdown;

    [Header("Debug")]
    //[SerializeField] private TMP_Text _fpsText;

    private bool _isVisible;
    public bool IsVisible => _isVisible;

    #region UNITY EVENT
    private void Start()
    {
        Initialize();
    }
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
        //_fpsText.text = $"FPS : {(int)GameSettingsManager.Instance.CurrentFPS}";
        if (keyboard.fKey.wasPressedThisFrame)
        {
            Debug.Log($"FPS : {(int)GameSettingsManager.Instance.CurrentFPS}");
        }   
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    #region INITIALIZE
    public void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        RegisterButtons();
        RegisterSettings();

        _tutorialPanel.SetActive(false);

        UpdateSceneUI(SceneManager.GetActiveScene().name);
        UpdateStageText();

        Hide();
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
        _bgmToggle.onValueChanged.RemoveAllListeners();
        _bgmToggle.onValueChanged.AddListener(SetBGM);

        _seToggle.onValueChanged.RemoveAllListeners();
        _seToggle.onValueChanged.AddListener(SetSE);

        _bgmSlider.onValueChanged.RemoveAllListeners();
        _bgmSlider.onValueChanged.AddListener(SetBGMVolume);

        _seSlider.onValueChanged.RemoveAllListeners();
        _seSlider.onValueChanged.AddListener(SetSEVolume);

        //_vSyncToggle.onValueChanged.RemoveAllListeners();
        //_vSyncToggle.onValueChanged.AddListener(SetVSync);

        _fpsDropdown.onValueChanged.RemoveAllListeners();
        _fpsDropdown.onValueChanged.AddListener(SetFPS);
    }
    #endregion

    #region SETTINGS
    private void SetBGM(bool value)
    {
        GameSettingsManager.Instance.SetBGMMute(!value);
        _bgmSlider.gameObject.SetActive(value);
    }
    private void SetBGMVolume(float value)
    {
        GameSettingsManager.Instance.SetBGMVolume(value);
    }
    private void SetSE(bool value)
    {
        GameSettingsManager.Instance.SetSEMute(!value);
        _seSlider.gameObject.SetActive(value);
    }
    private void SetSEVolume(float value) 
    { 
        GameSettingsManager.Instance.SetSEVolume(value);
    }
    private void SetVSync(bool value)
    {
        GameSettingsManager.Instance.SetVSync(value);
    }
    private void SetFPS(int value)
    {
        switch (value)
        {
            case 0:
                GameSettingsManager.Instance.SetFPS(30);
                break;

            case 1:
                GameSettingsManager.Instance.SetFPS(60);
                break;

            case 2:
                GameSettingsManager.Instance.SetFPS(144);
                break;

            case 3:
                GameSettingsManager.Instance.SetFPS(-1);
                break;
        }
    }
    #endregion 
    #region MENU
    public void ToggleMenu()
    {
        if (_isVisible) Hide();
        else Show();

        GameController.Instance.ToggleMenu();
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

        GameController.Instance.ToggleMenu();
        GameManager.Instance.LoadStageSelect();
    }

    private void OpenTitle()
    {
        Hide();

        GameController.Instance.ToggleMenu();
        GameManager.Instance.LoadStart();
    }
    #endregion

    #region SCENE
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSceneUI(scene.name);
        UpdateStageText();
        Hide();
    }
    private void UpdateSceneUI(string sceneName)
    {
        _inGameGroup.SetActive(sceneName == "InGame");
        _stageSelectGroup.SetActive(sceneName == "StageSelect");
        _startGroup.SetActive(sceneName == "Start");
    }
    private void UpdateStageText()
    {
        _currentStageText.text = $"STAGE {GameManager.Instance.CurrentStageID:00}";
    }
    #endregion
}