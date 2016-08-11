// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EndlessClient.Controllers.Repositories;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Login;
using EOLib.Graphics;

namespace EndlessClient.UIControls
{
    public class CharacterInfoPanelFactory : ICharacterInfoPanelFactory
    {
        private readonly ICharacterSelectorProvider _characterProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ILoginControllerRepository _loginControllerRepository;
        private readonly ICharacterManagementControllerRepository _characterManagementControllerRepository;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly ICharacterRendererResetter _characterRendererResetter;

        public CharacterInfoPanelFactory(ICharacterSelectorProvider characterProvider,
                                         INativeGraphicsManager nativeGraphicsManager,
                                         ILoginControllerRepository loginControllerRepository,
                                         ICharacterManagementControllerRepository characterManagementControllerRepository,
                                         ICharacterRendererFactory characterRendererFactory,
                                         ICharacterRendererResetter characterRendererResetter)
        {
            _characterProvider = characterProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _loginControllerRepository = loginControllerRepository;
            _characterManagementControllerRepository = characterManagementControllerRepository;
            _characterRendererFactory = characterRendererFactory;
            _characterRendererResetter = characterRendererResetter;
        }

        public IEnumerable<CharacterInfoPanel> CreatePanels()
        {
            int i = 0;
            for (; i < _characterProvider.Characters.Count; ++i)
            {
                var character = _characterProvider.Characters[i];
                yield return new CharacterInfoPanel(i,
                                                    character,
                                                    _nativeGraphicsManager,
                                                    _loginControllerRepository.LoginController,
                                                    _characterManagementControllerRepository.CharacterManagementController,
                                                    _characterRendererFactory,
                                                    _characterRendererResetter);
            }

            for (; i < 3; ++i)
                yield return new EmptyCharacterInfoPanel(i, _nativeGraphicsManager);
        }
    }
}