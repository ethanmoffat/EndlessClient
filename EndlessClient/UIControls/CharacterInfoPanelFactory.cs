using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Services;
using EndlessClient.Input;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using EOLib.Graphics;

namespace EndlessClient.UIControls
{
    [AutoMappedType(IsSingleton = true)]
    public class CharacterInfoPanelFactory : ICharacterInfoPanelFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IRendererRepositoryResetter _rendererRepositoryResetter;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly IUserInputProvider _userInputProvider;
        private readonly IXnaControlSoundMapper _xnaControlSoundMapper;
        private ILoginController _loginController;
        private ICharacterManagementController _characterManagementController;

        public CharacterInfoPanelFactory(INativeGraphicsManager nativeGraphicsManager,
                                         ICharacterRendererFactory characterRendererFactory,
                                         IRendererRepositoryResetter rendererRepositoryResetter,
                                         IEODialogButtonService eoDialogButtonService,
                                         IUserInputProvider userInputProvider,
                                         IXnaControlSoundMapper xnaControlSoundMapper)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterRendererFactory = characterRendererFactory;
            _rendererRepositoryResetter = rendererRepositoryResetter;
            _eoDialogButtonService = eoDialogButtonService;
            _userInputProvider = userInputProvider;
            _xnaControlSoundMapper = xnaControlSoundMapper;
        }

        public void InjectLoginController(ILoginController loginController)
        {
            _loginController = loginController;
        }

        public void InjectCharacterManagementController(ICharacterManagementController characterManagementController)
        {
            _characterManagementController = characterManagementController;
        }

        public IEnumerable<CharacterInfoPanel> CreatePanels(IEnumerable<Character> characters)
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
                                                    _rendererRepositoryResetter,
                                                    _userInputProvider,
                                                    _xnaControlSoundMapper);
            }

            for (; i < 3; ++i)
                yield return new EmptyCharacterInfoPanel(i, _nativeGraphicsManager, _eoDialogButtonService);
        }
    }
}