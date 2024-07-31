using AutomaticTypeMapper;

namespace EndlessClient.GameExecution
{
    public interface IGameStateRepository
    {
        GameStates CurrentState { get; set; }
    }

    public interface IGameStateProvider
    {
        GameStates CurrentState { get; }
    }

    [MappedType(BaseType = typeof(IGameStateProvider), IsSingleton = true)]
    [MappedType(BaseType = typeof(IGameStateRepository), IsSingleton = true)]
    public class GameStateRepository : IGameStateRepository, IGameStateProvider
    {
        public GameStates CurrentState { get; set; }
    }
}