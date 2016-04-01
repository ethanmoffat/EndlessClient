// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.Game
{
	public interface IGameStateRepository
	{
		GameStates CurrentState { get; set; }
	}

	public interface IGameStateProvider
	{
		GameStates CurrentState { get; }
	}

	public class GameStateRepository : IGameStateRepository, IGameStateProvider
	{
		public GameStates CurrentState { get; set; }
	}
}
