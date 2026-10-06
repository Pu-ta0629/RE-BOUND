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
    private void Start()
    {
        Instance = this;

        _retryManager.Initialize();
        _stageProgressManager.Initialize();

        _stageManager.Initialize(GameManager.Instance.CurrentStageID);
        _playerManager.Initialize(_stageManager.CurrentStageData);
        _gimmickManager.Initialize();
    }
}