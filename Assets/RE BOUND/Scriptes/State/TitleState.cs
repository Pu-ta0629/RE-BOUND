using UnityEngine;

public class TitleState : GameStateBase
{
    [SerializeField] private StartSceneController _titleController;
    [SerializeField] private PlayerManager _playerManager;

    public override Enum_GameState StateType => Enum_GameState.Title;

    protected override void OnEnter()
    {
        _titleController.Begin();
        _playerManager.BeginFree(() => _titleController.IsBusy);
    }
}