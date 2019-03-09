// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
