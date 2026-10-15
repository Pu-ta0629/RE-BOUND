using UnityEngine;

public class StageSelectState : GameStateBase
{
    [SerializeField] private StageSelectManager _stageSelectManager;

    public override Enum_GameState StateType => Enum_GameState.StageSelect;

    protected override void OnEnter()
    {
        _stageSelectManager.Begin();
    }
}
