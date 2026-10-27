using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("RE:BOUND")]
    [SerializeField] private int _currentStageID;
    [SerializeField] private int _maxUnlockStage;
    public int CurrentStageID { get; private set; }
    public int MaxUnlockStage { get; private set; }
    [Space(10)]


    [SerializeField] private Camera _mainCamera;
    public Camera MainCamera => _mainCamera;
    [Space(10)]

    [Header("Ref")]
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private ButtonSE _buttonSE;
    [SerializeField] private EffectManager _effectManager;
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameSettingsManager _gameSettingsManager;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private MenuUI _menuUI;
    [SerializeField] private StageDatabase _stageDatabase;
    [SerializeField] private StarTooltip _starTooltip;
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private SaveManager _saveManager;

    [Tooltip("ステージの読み込み直し(Retry)を、次に受けるまでの秒数")]
    [SerializeField] private float _reloadCooltime = 1.5f;
    private float _nextReloadTime;

    public SaveManager SaveManager => _saveManager;
    public ButtonSE ButtonSE => _buttonSE;
    public StageData GetStageData(int stageID) => _stageDatabase != null ? _stageDatabase.Get(stageID) : null;

    // 唯一のAwake
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (!NULLCHECK()) return; //not true

        Initialize();

        _currentStageID = CurrentStageID;
        _maxUnlockStage = MaxUnlockStage;
    }

    private void Initialize()
    {
        _saveManager.Initialize();
        _gameSettingsManager.Initialize();
        _effectManager.Initialize();
        _audioManager.Initialize();

        //セーブデータ
        Load();

        //ゲーム本体
        _gameController.Initialize();

        //UI更新
        _starTooltip.Initialize();

        //State(ゲーム管理
        _stateManager.Initialize();

        //Stateに応じてMenuを変化させるためStateの後
        _menuUI.Initialize();

        //全State共通の入力
        _inputManager.Initialize();

        //ここで画面表示
        _stateManager.Begin();
    }

    private bool NULLCHECK()
    {
        bool ok = true;

        ok &= CHECK(_saveManager, nameof(_saveManager));
        ok &= CHECK(_gameSettingsManager, nameof(_gameSettingsManager));
        ok &= CHECK(_effectManager, nameof(_effectManager));
        ok &= CHECK(_audioManager, nameof(_audioManager));
        ok &= CHECK(_buttonSE, nameof(_buttonSE));
        ok &= CHECK(_inputManager, nameof(_inputManager));
        ok &= CHECK(_gameController, nameof(_gameController));
        ok &= CHECK(_starTooltip, nameof(_starTooltip));
        ok &= CHECK(_stateManager, nameof(_stateManager));
        ok &= CHECK(_menuUI, nameof(_menuUI));
        ok &= CHECK(_mainCamera, nameof(_mainCamera));

        return ok;
    }

    private bool CHECK(Object target, string fieldName)
    {
        if (target != null) return true;

        Debug.LogError($"{this.name} : {fieldName} が未設定です");
        return false;
    }

    #region Stage

    public void SetCurrentStage(int stageID)
    {
        CurrentStageID = stageID;
        _currentStageID = stageID;
    }

    public void UnlockStage(int stageID)
    {
        if (stageID < MaxUnlockStage) return;

        MaxUnlockStage = stageID;
        _maxUnlockStage = stageID;

        Save();
    }

    #endregion

    #region ChangeState

    public void GoTitle() => StateManager.Instance.ChangeState(Enum_GameState.Title);

    public void GoStageSelect() => StateManager.Instance.ChangeState(Enum_GameState.StageSelect);

    public void GoInGame() => StateManager.Instance.ChangeState(Enum_GameState.InGame);

    // 今のステージを最初から読み込み直す（InGame に入り直す）
    public void ReloadStage()
    {
        if (Time.unscaledTime < _nextReloadTime) return;
        _nextReloadTime = Time.unscaledTime + _reloadCooltime;
        AudioManager.Instance.Play(Enum_SEType.Retry);
        StateManager.Instance.ChangeState(Enum_GameState.InGame, true); 
    }

    #endregion

    #region Save
    //Play内容をセーブ（保存）
    public void Save()
    {
        SaveData data = _saveManager.Load();

        data.CurrentStageID = CurrentStageID;
        data.MaxUnlockedStage = MaxUnlockStage;

        _saveManager.Save(data);
    }

    //セーブデータのロード（読み込み）
    public void Load()
    {
        SaveData data = _saveManager.Load();

        CurrentStageID = data.CurrentStageID;
        MaxUnlockStage = data.MaxUnlockedStage;
    }

    #endregion

    #region QUIT
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
}