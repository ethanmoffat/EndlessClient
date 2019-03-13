using System.Collections;
using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Controls;
using EndlessClient.Input;
using EndlessClient.Network;
using EndlessClient.UIControls;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Initialization
{
    [MappedType(BaseType = typeof(IGameInitializer))]
    public class EndlessClientInitializer : IGameInitializer
    {
        private readonly IEndlessGame _game;
        private readonly IEndlessGameRepository _endlessGameRepository;
        private readonly IContentManagerRepository _contentManagerRepository;
        private readonly IKeyboardDispatcherRepository _keyboardDispatcherRepository;
        private readonly PacketHandlerGameComponent _packetHandlerGameComponent;

        private readonly IMainButtonController _mainButtonController;
        private readonly IAccountController _accountController;
        private readonly ILoginController _loginController;
        private readonly ICharacterManagementController _characterManagementController;
        private readonly IChatController _chatController;
        private readonly IControlSetFactory _controlSetFactory;
        private readonly ICharacterInfoPanelFactory _characterInfoPanelFactory;
        private readonly IHudControlsFactory _hudControlsFactory;

        public EndlessClientInitializer(IEndlessGame game,
                                        IEndlessGameRepository endlessGameRepository,
                                        IContentManagerRepository contentManagerRepository,
                                        IKeyboardDispatcherRepository keyboardDispatcherRepository,
                                        PacketHandlerGameComponent packetHandlerGameComponent,

                                        //Todo: refactor method injection to something like IEnumerable<IMethodInjectable>
                                        IMainButtonController mainButtonController,
                                        IAccountController accountController,
                                        ILoginController loginController,
                                        ICharacterManagementController characterManagementController,
                                        IChatController chatController,
                                        IControlSetFactory controlSetFactory,
                                        ICharacterInfoPanelFactory characterInfoPanelFactory,
                                        IHudControlsFactory hudControlsFactory)
        {
            _game = game;
            _endlessGameRepository = endlessGameRepository;
            _contentManagerRepository = contentManagerRepository;
            _keyboardDispatcherRepository = keyboardDispatcherRepository;
            _packetHandlerGameComponent = packetHandlerGameComponent;
            _mainButtonController = mainButtonController;
            _accountController = accountController;
            _loginController = loginController;
            _characterManagementController = characterManagementController;
            _chatController = chatController;
            _controlSetFactory = controlSetFactory;
            _characterInfoPanelFactory = characterInfoPanelFactory;
            _hudControlsFactory = hudControlsFactory;
        }

        public void Initialize()
        {
            GameRepository.SetGame(_game as Game);

            _game.Components.Add(_packetHandlerGameComponent);
            _endlessGameRepository.Game = _game;

            _contentManagerRepository.Content = _game.Content;
            _contentManagerRepository.Content.RootDirectory = "ContentPipeline";

            _keyboardDispatcherRepository.Dispatcher = new KeyboardDispatcher(_game.Window);

            _controlSetFactory.InjectControllers(_mainButtonController,
                                                 _accountController,
                                                 _loginController,
                                                 _characterManagementController);
            _characterInfoPanelFactory.InjectCharacterManagementController(_characterManagementController);
            _characterInfoPanelFactory.InjectLoginController(_loginController);
            _hudControlsFactory.InjectChatController(_chatController);
        }
    }
}
