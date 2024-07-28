namespace EndlessClient.GameExecution;

public interface IGameStateActions
{
    void ChangeToState(GameStates newState);

    void RefreshCurrentState();

    void ExitGame();
}