// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using EOLib.IO.Repositories;

namespace EndlessClient
{
    //todo: move this to a different namespace
    public class TestModeLauncher : ITestModeLauncher
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEIFFileProvider _eifFileProvider;

        public TestModeLauncher(IEndlessGameProvider endlessGameProvider,
                                ICharacterRendererFactory characterRendererFactory,
                                ICharacterRepository characterRepository,
                                IEIFFileProvider eifFileProvider)
        {
            _endlessGameProvider = endlessGameProvider;
            _characterRendererFactory = characterRendererFactory;
            _characterRepository = characterRepository;
            _eifFileProvider = eifFileProvider;
        }

        public void LaunchTestMode()
        {
            if (_characterRepository.ActiveCharacter == null)
                _characterRepository.ActiveCharacter = new EOLib.Domain.Character.Character()
                    .WithRenderProperties(new CharacterRenderProperties());

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
