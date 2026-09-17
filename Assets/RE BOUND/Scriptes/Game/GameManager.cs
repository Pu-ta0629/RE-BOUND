using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int CurrentStageID { get; private set; }
    public int MaxUnlockStage { get; private set; }

    [SerializeField] private int _currentStageID;
    [SerializeField] private int _maxUnlockStage;

    [SerializeField] private StaticSceneAsset _startScene;
    [SerializeField] private StaticSceneAsset _inGameScene;
    [SerializeField] private StaticSceneAsset _stageSelect;
    private SaveManager _saveManager;
    private GameSettingsManager _gameSettingsManager;
    private EffectManager _effectManager;
    public SaveManager SaveManager => _saveManager;
    public StaticSceneAsset StartScene => _startScene;
    public StaticSceneAsset InGameScene => _inGameScene;
    public StaticSceneAsset StageSelect => _stageSelect;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
        _currentStageID = CurrentStageID;
        _maxUnlockStage = MaxUnlockStage;

    }
    private void Initialize()
    {
        _saveManager = GetComponent<SaveManager>();
        _gameSettingsManager = GetComponent<GameSettingsManager>();
        _effectManager = GetComponentInChildren<EffectManager>();

        _saveManager.Initialize();
        _gameSettingsManager.Initialize();
        _effectManager.Initialize();
        
        Load();

        //FindFirstObjectByType<GameController>()?.Initialize();
    }

    #region Stage

    public void SetCurrentStage(int stageID)
    {
        CurrentStageID = stageID;
    }

    public void UnlockStage(int stageID)
    {
        if (stageID < MaxUnlockStage)
            return;

        MaxUnlockStage = stageID;

        Save();
    }

    #endregion

    #region Scene

    public void LoadStart()
    {
        Debug.Log("LOAD SATRT");
        SceneManager.LoadScene(_startScene.Value);
    }

    public void LoadStageSelect()
    {
        Debug.Log("LOAD STAGE SELECT");
        SceneManager.LoadScene(_stageSelect.Value);
    }

    public void LoadInGame()
    {
        Debug.Log("LOAD INGAME");
        SceneManager.LoadScene(_inGameScene.Value);
    }

    #endregion

    #region Save
    //保存
    public void Save()
    {
        SaveData data = _saveManager.Load();
        data.CurrentStageID = CurrentStageID;
        data.MaxUnlockedStage = MaxUnlockStage;
        _saveManager.Save(data);
    }
    //保存データの読み込み
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
        Debug.Log("END GAME");
        //UnityEditor と Application時に終了の処理を変える
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
#endregion
}