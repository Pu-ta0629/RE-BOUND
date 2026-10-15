using UnityEngine;

public class InGameState : GameStateBase
{
    [SerializeField] private GameController _gameController;

    public override Enum_GameState StateType => Enum_GameState.InGame;

    protected override void OnEnter()
    {
        _gameController.BeginGame(GameManager.Instance.CurrentStageID);
    }

    protected override void OnExit()
    {
        _gameController.EndGame();
    }
}
