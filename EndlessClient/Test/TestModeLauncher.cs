using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Factories;
using EOLib.IO.Repositories;

namespace EndlessClient.Test
{
    [MappedType(BaseType = typeof(ITestModeLauncher))]
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
