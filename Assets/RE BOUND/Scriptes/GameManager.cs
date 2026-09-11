using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.InputSystem;
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
        if(_saveManager == null)
        {
            _saveManager = gameObject.AddComponent<SaveManager>();
        }
        _saveManager.Initialize();

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
        if (stageID <= MaxUnlockStage)
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

    public void Save()
    {
        SaveData data = _saveManager.Load();
        data.CurrentStageID = CurrentStageID;
        data.MaxUnlockedStage = MaxUnlockStage;
        _saveManager.Save(data);
    }

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
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
#endregion
}

//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.InputSystem;
//public class GameManager : MonoBehaviour
//{
//    [Header("Reference")]
//    [SerializeField] private PlayerManager _playerManager;

//    [Header("Stage")]
//    [SerializeField] private StageData _stageData;

//    [Header("UI")]
//    [SerializeField] private Retry _retry;
//    private PlayerMovement _player;

//    #region UNITY EVENT
//    private void Awake()
//    {
//        NULLCHECK();

//        if (_retry != null)
//        {
//            _retry.OnRetry += Retry;
//        }

//        SpawnPlayer();
//    }

//    private void OnDestroy()
//    {
//        if (_retry != null)
//        {
//            _retry.OnRetry -= Retry;
//        }
//    }

//    private void Update()
//    {
//        if (Keyboard.current?.rKey.wasPressedThisFrame == true)
//        {
//            Retry();
//        }
//    }
//    #endregion

//    private void SpawnPlayer()
//    {
//        _player = _playerManager.SpawnPlayer(_stageData);
//    }

//    public void Retry()
//    {
//        _playerManager.Initialize(_stageData);
//    }

//    private void NULLCHECK()
//    {
//        if (_playerManager == null)
//        {
//            Debug.LogError("PlayerManager is NULL");
//        }
//        if (_stageData == null)
//        {
//            Debug.LogError("StageData is NULL");
//        }
//    }
//}