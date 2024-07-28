namespace EndlessClient.GameExecution;

public interface IGameRunner
{
    bool SetupDependencies();

    void RunGame();
}