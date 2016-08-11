// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EndlessClient.Rendering.Factories;
using EOLib.IO.Repositories;

namespace EndlessClient
{
    //todo: move this to a different namespace
    public class TestModeLauncher : ITestModeLauncher
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IGameStateProvider _gameStateProvider;

        public TestModeLauncher(IEndlessGameProvider endlessGameProvider,
                                ICharacterRendererFactory characterRendererFactory,
                                IEIFFileProvider eifFileProvider,
                                IGameStateProvider gameStateProvider)
        {
            _endlessGameProvider = endlessGameProvider;
            _characterRendererFactory = characterRendererFactory;
            _eifFileProvider = eifFileProvider;
            _gameStateProvider = gameStateProvider;
        }

        public void LaunchTestMode()
        {
            if (_gameStateProvider.CurrentState != GameStates.None)
                return;

            var testMode = new CharacterStateTest(
                _endlessGameProvider.Game,
                _characterRendererFactory,
                _eifFileProvider);

            _endlessGameProvider.Game.Components.Clear();
            _endlessGameProvider.Game.Components.Add(testMode);
        }
    }

    public interface ITestModeLauncher
    {
        void LaunchTestMode();
    }
}
