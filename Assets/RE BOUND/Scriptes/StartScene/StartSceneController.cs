using UnityEngine;
using UnityEngine.UI;

public class StartSceneController : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button _gameButton;
    [SerializeField] private Button _stageSelectButton;
    [SerializeField] private Button _quitButton;

    [Header("Animation")]
    [SerializeField] private Animator _canvasAnimator;

    private void Start()
    {
        Initialize();
    }
    public void Initialize()
    {
        _gameButton.onClick.AddListener(GameButtonAnimation);
        _stageSelectButton.onClick.AddListener(StageSelectButtonAnimation);
        _quitButton.onClick.AddListener(QuitButton);
    }
    private void GameButtonAnimation() =>_canvasAnimator.SetBool("IsGame", true);
    private void StageSelectButtonAnimation() => _canvasAnimator.SetBool("IsStageSelect", true);
    private void QuitButton() => _canvasAnimator.SetBool("IsQuit", true);
    public void LoadInGame() => GameManager.Instance.LoadInGame();
    public void LoadStageSelect() => GameManager.Instance.LoadStageSelect();
    public void LoadQuit() => GameManager.Instance.Quit();
}