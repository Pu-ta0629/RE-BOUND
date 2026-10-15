using UnityEngine;

public class TitleState : GameStateBase
{
    [SerializeField] private StartSceneController _titleController;

    public override Enum_GameState StateType => Enum_GameState.Title;

    protected override void OnEnter()
    {
        _titleController.Begin();
    }
}