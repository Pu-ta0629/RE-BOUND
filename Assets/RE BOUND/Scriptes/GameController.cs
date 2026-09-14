using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Reference")]
    [SerializeField] private StageManager _stageManager;
    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private GimmickManager _gimmickManager;
    [SerializeField] private Button _retryButton;
    public StageManager StageManager => _stageManager;
    public PlayerManager PlayerManager => _playerManager;
    public GimmickManager GimmickManager => _gimmickManager;

    private bool _isPlaying = true;

    public bool IsPlaying => _isPlaying;

    public void Initialize()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _retryButton.onClick.RemoveAllListeners();
        _retryButton.onClick.AddListener(Retry);

        Instance = this;

        _stageManager.Initialize(GameManager.Instance.CurrentStageID);

        _playerManager.Initialize(_stageManager.CurrentStageData);

        _gimmickManager.Initialize();
        Debug.Log(_stageManager);
        Debug.Log(_playerManager);
        Debug.Log(_gimmickManager);
    }
    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ActiveEvent();
        }

        if(Keyboard.current.rKey.wasPressedThisFrame)
        {
            Retry();
        }
    }

    #region Retry

    public void Retry()
    {
        _playerManager.Initialize(_stageManager.CurrentStageData);
    }

    #endregion

    #region Goal

    public void NextStage()
    {
        int bounceCount = _playerManager.Player.BounceCount;
        GameManager.Instance.SaveManager.UpdateBestBounce(GameManager.Instance.CurrentStageID, bounceCount);

        int nextStage = GameManager.Instance.CurrentStageID + 1;

        if (!_stageManager.IsExistStage(nextStage))
        {
            GameManager.Instance.LoadStart();
            return;
        }

        GameManager.Instance.UnlockStage(nextStage);

        GameManager.Instance.SetCurrentStage(nextStage);

        _stageManager.LoadStage(nextStage);

        _gimmickManager.Initialize();

        Retry();
    }

    #endregion

    #region Pause

    public void ToggleMenu()
    {
        _isPlaying = !_isPlaying;

        Debug.Log(_isPlaying ? "Resume" : "Pause");
    }

    #endregion

    #region ActiveEvent

    public void ActiveEvent()
    {
        _gimmickManager.InvokeActiveEvent();
    }

    #endregion
}