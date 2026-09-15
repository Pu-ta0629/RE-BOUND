using UnityEngine;
using UnityEngine.UI;

public class StartSceneController : MonoBehaviour
{
    [SerializeField] private Button _game;
    [SerializeField] private Button _stageSelect;
    [SerializeField] private Button _quit;

    private void Start()
    {
        Initialize();
    }
    public void Initialize()
    {
        _game.onClick.AddListener(GameManager.Instance.LoadInGame);
        _stageSelect.onClick.AddListener(GameManager.Instance.LoadStageSelect);
        _quit.onClick.AddListener(GameManager.Instance.Quit);
    }
}