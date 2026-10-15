using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    [Header("InGame System")]
    [SerializeField] private RetryManager _retryManager;
    [SerializeField] private StageProgressManager _stageProgressManager;
    [SerializeField] private StageManager _stageManager;
    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private GimmickManager _gimmickManager;
    public bool IsPlaying => !GameSettingsManager.Instance.IsPaused;
    public void Initialize()
    {
        Instance = this;

        _retryManager.Initialize();
        _stageProgressManager.Initialize();
        _stageManager.Initialize()
            ;
        //_retryManager.Initialize();
        //_stageProgressManager.Initialize();

        //_stageManager.Initialize(GameManager.Instance.CurrentStageID);
        //_playerManager.Initialize(_stageManager.CurrentStageData);
        //_gimmickManager.Initialize();
    }
    private void NULLCHECK()
    {
        if(StarTooltip.Instance == null)
        {
            Debug.LogWarning($"{name} : Startooltip not found");
        }
    }

    public void BeginGame(int stageID)
    {
        _retryManager.ResetState();
        _stageProgressManager.ResetState();

        LoadStage(stageID);

        _playerManager.Initialize(_stageManager.CurrentStageData);
    }
    public void EndGame()
    {
        _retryManager.ResetState();
        _stageProgressManager.ResetState();

        GameSettingsManager.Instance.SetPause(false);

        NULLCHECK();
        StarTooltip.Instance.Hide();
    }
    public void LoadStage(int stageID)
    {
        _stageManager.LoadStage(stageID);
        _gimmickManager.Initialize();
    }
}