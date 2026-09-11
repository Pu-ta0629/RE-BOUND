public interface IGimmick
{
    void Initialize();
    void OnPlayerHit(PlayerMovement player);
    void OnPlayerMiss(PlayerMovement player);
    void OnActiveEvent();
}
