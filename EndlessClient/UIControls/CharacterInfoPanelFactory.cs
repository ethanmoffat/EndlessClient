using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Services;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Graphics;

namespace EndlessClient.UIControls
{
    [AutoMappedType(IsSingleton = true)]
    public class CharacterInfoPanelFactory : ICharacterInfoPanelFactory
    {
        private readonly ICharacterSelectorProvider _characterProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IRendererRepositoryResetter _rendererRepositoryResetter;
        private readonly IEODialogButtonService _eoDialogButtonService;

        private ILoginController _loginController;
        private ICharacterManagementController _characterManagementController;

        public CharacterInfoPanelFactory(ICharacterSelectorProvider characterProvider,
                                         INativeGraphicsManager nativeGraphicsManager,
                                         ICharacterRendererFactory characterRendererFactory,
                                         IRendererRepositoryResetter rendererRepositoryResetter,
                                         IEODialogButtonService eoDialogButtonService)
        {
            _characterProvider = characterProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterRendererFactory = characterRendererFactory;
            _rendererRepositoryResetter = rendererRepositoryResetter;
            _eoDialogButtonService = eoDialogButtonService;
        }

        public void InjectLoginController(ILoginController loginController)
        {
            _loginController = loginController;
        }

        public void InjectCharacterManagementController(ICharacterManagementController characterManagementController)
        {
            _characterManagementController = characterManagementController;
        }

        public IEnumerable<CharacterInfoPanel> CreatePanels(IEnumerable<ICharacter> characters)
        {
            if(_loginController == null || _characterManagementController == null)
                throw new InvalidOperationException("Missing controllers - the Unity container was initialized incorrectly");

            int i = 0;
            foreach (var character in characters)
            {
                yield return new CharacterInfoPanel(i++,
                                                    character,
                                                    _nativeGraphicsManager,
                                                    _eoDialogButtonService,
                                                    _loginController,
                                                    _characterManagementController,
                                                    _characterRendererFactory,
                                                    _rendererRepositoryResetter);
            }

            for (; i < 3; ++i)
                yield return new EmptyCharacterInfoPanel(i, _nativeGraphicsManager, _eoDialogButtonService);
        }
    }
}