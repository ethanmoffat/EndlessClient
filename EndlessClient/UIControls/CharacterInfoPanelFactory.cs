// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.Controllers;
using EndlessClient.Rendering;
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
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IRendererRepositoryResetter _rendererRepositoryResetter;

        private ILoginController _loginController;
        private ICharacterManagementController _characterManagementController;

        public CharacterInfoPanelFactory(ICharacterSelectorProvider characterProvider,
                                         INativeGraphicsManager nativeGraphicsManager,
                                         ICharacterRendererFactory characterRendererFactory,
                                         IRendererRepositoryResetter rendererRepositoryResetter)
        {
            _characterProvider = characterProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterRendererFactory = characterRendererFactory;
            _rendererRepositoryResetter = rendererRepositoryResetter;
        }

        public void InjectLoginController(ILoginController loginController)
        {
            _loginController = loginController;
        }

        public void InjectCharacterManagementController(ICharacterManagementController characterManagementController)
        {
            _characterManagementController = characterManagementController;
        }

        public IEnumerable<CharacterInfoPanel> CreatePanels()
        {
            if(_loginController == null || _characterManagementController == null)
                throw new InvalidOperationException("Missing controllers - the Unity container was initialized incorrectly");

            int i = 0;
            for (; i < _characterProvider.Characters.Count; ++i)
            {
                var character = _characterProvider.Characters[i];
                yield return new CharacterInfoPanel(i,
                                                    character,
                                                    _nativeGraphicsManager,
                                                    _loginController,
                                                    _characterManagementController,
                                                    _characterRendererFactory,
                                                    _rendererRepositoryResetter);
            }

            for (; i < 3; ++i)
                yield return new EmptyCharacterInfoPanel(i, _nativeGraphicsManager);
        }
    }
}