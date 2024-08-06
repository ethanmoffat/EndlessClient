using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Metadata;
using EndlessClient.Rendering.Metadata.Models;
using EOLib.IO.Repositories;

namespace EndlessClient.Test
{
    [AutoMappedType]
    public class TestModeLauncher : ITestModeLauncher
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IMetadataProvider<WeaponMetadata> _weaponMetadataProvider;

        public TestModeLauncher(IEndlessGameProvider endlessGameProvider,
                                ICharacterRendererFactory characterRendererFactory,
                                IEIFFileProvider eifFileProvider,
                                IGameStateProvider gameStateProvider,
                                IMetadataProvider<WeaponMetadata> weaponMetadataProvider)
        {
            _endlessGameProvider = endlessGameProvider;
            _characterRendererFactory = characterRendererFactory;
            _eifFileProvider = eifFileProvider;
            _gameStateProvider = gameStateProvider;
            _weaponMetadataProvider = weaponMetadataProvider;
        }

        public void LaunchTestMode()
        {
            if (_gameStateProvider.CurrentState != GameStates.None)
                return;

            var testMode = new CharacterStateTest(
                _endlessGameProvider.Game,
                _characterRendererFactory,
                _eifFileProvider,
                _weaponMetadataProvider);

            _endlessGameProvider.Game.Components.Clear();
            _endlessGameProvider.Game.Components.Add(testMode);
        }
    }

    public interface ITestModeLauncher
    {
        void LaunchTestMode();
    }
}
