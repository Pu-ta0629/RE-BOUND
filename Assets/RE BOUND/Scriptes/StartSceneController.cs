using UnityEngine;
using UnityEngine.UI;

public class StartSceneController : MonoBehaviour
{
    [SerializeField] private Button _game;
    [SerializeField] private Button _stageSelect;
    [SerializeField] private Button _quit; 
    [SerializeField] private GameManager _gameManager;

    private void Start()
    {
        _game.onClick        .AddListener(_gameManager.LoadInGame);
        _stageSelect.onClick .AddListener(_gameManager.LoadStageSelect);
        _quit.onClick        .AddListener(_gameManager.Quit);
    }
}
