using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Net.Communication;
using EOLib.Net.Handlers;
using Microsoft.Xna.Framework;

namespace EndlessClient.Network
{
    [MappedType(BaseType = typeof(IGameComponent), IsSingleton = true)]
    public class PacketHandlerGameComponent : GameComponent
    {
        private readonly IOutOfBandPacketHandler _packetHandler;
        private readonly INetworkClientProvider _networkClientProvider;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IErrorDialogDisplayAction _errorDialogDisplayAction;
        private readonly IMainButtonController _mainButtonController;

        public PacketHandlerGameComponent(IEndlessGame game,
                                          IOutOfBandPacketHandler packetHandler,
                                          INetworkClientProvider networkClientProvider,
                                          IGameStateProvider gameStateProvider,
                                          IErrorDialogDisplayAction errorDialogDisplayAction,
                                          IMainButtonController mainButtonController)
            : base((Game) game)
        {
            _packetHandler = packetHandler;
            _networkClientProvider = networkClientProvider;
            _gameStateProvider = gameStateProvider;
            _errorDialogDisplayAction = errorDialogDisplayAction;
            _mainButtonController = mainButtonController;

            UpdateOrder = int.MinValue;
        }

        public override void Update(GameTime gameTime)
        {
            if (_networkClientProvider.NetworkClient != null &&
                _networkClientProvider.NetworkClient.Started &&
                !_networkClientProvider.NetworkClient.Connected)
            {
                var isInGame = _gameStateProvider.CurrentState == GameStates.PlayingTheGame;
                _mainButtonController.GoToInitialStateAndDisconnect();
                _errorDialogDisplayAction.ShowConnectionLost(isInGame);
            }

            _packetHandler.PollForPacketsAndHandle();

            base.Update(gameTime);
        }
    }
}
