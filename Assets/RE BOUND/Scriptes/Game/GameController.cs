using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [SerializeField] private StageManager _stageManager;

    [SerializeField] private PlayerManager _playerManager;

    [SerializeField] private GimmickManager _gimmickManager;

    public bool IsPlaying => !GameSettingsManager.Instance.IsPaused;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _stageManager.Initialize(GameManager.Instance.CurrentStageID);

        _playerManager.Initialize(_stageManager.CurrentStageData);

        _gimmickManager.Initialize();
    }
}