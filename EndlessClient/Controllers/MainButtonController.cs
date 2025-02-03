using System;
using System.Threading;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EOLib.Domain;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.PacketProcessing;
using EOLib.Shared;
using Moffat.EndlessOnline.SDK.Packet;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EndlessClient.Controllers
{
    [AutoMappedType(IsSingleton = true)]
    public class MainButtonController : IMainButtonController
    {
        private readonly INetworkConnectionActions _networkConnectionActions;
        private readonly IErrorDialogDisplayAction _errorDialogDisplayAction;
        private readonly IPacketProcessActions _packetProcessActions;
        private readonly IBackgroundReceiveActions _backgroundReceiveActions;
        private readonly IGameStateActions _gameStateActions;
        private readonly IAccountDialogDisplayActions _accountDialogDisplayActions;
        private readonly IResetStateAction _resetStateAction;
        private readonly ISafeNetworkOperationFactory _networkOperationFactory;

        private readonly Random _random;

        private int _numberOfConnectionRequests;

        public MainButtonController(INetworkConnectionActions networkConnectionActions,
                                    IErrorDialogDisplayAction errorDialogDisplayAction,
                                    IPacketProcessActions packetProcessActions,
                                    IBackgroundReceiveActions backgroundReceiveActions,
                                    IGameStateActions gameStateActions,
                                    IAccountDialogDisplayActions accountDialogDisplayActions,
                                    IResetStateAction resetStateAction,
                                    ISafeNetworkOperationFactory networkOperationFactory)
        {
            _networkConnectionActions = networkConnectionActions;
            _errorDialogDisplayAction = errorDialogDisplayAction;
            _packetProcessActions = packetProcessActions;
            _backgroundReceiveActions = backgroundReceiveActions;
            _gameStateActions = gameStateActions;
            _accountDialogDisplayActions = accountDialogDisplayActions;
            _resetStateAction = resetStateAction;
            _networkOperationFactory = networkOperationFactory;

            _random = new Random();
        }

        public void GoToInitialState()
        {
            _gameStateActions.ChangeToState(GameStates.Initial);
        }

        public void GoToInitialStateAndDisconnect(bool showLostConnection = false)
        {
            GoToInitialState();
            StopReceivingAndDisconnect();

            _resetStateAction.ResetState();

            if (showLostConnection)
                _errorDialogDisplayAction.ShowConnectionLost(false);
        }

        public async Task ClickCreateAccount()
        {
            var result = await StartNetworkConnection().ConfigureAwait(false);

            if (result)
            {
                await DispatcherGameComponent.InvokeAsync(() =>
                {
                    _gameStateActions.ChangeToState(GameStates.CreateAccount);
                    _accountDialogDisplayActions.ShowInitialCreateWarningDialog();
                });
            }
        }

        public async Task ClickLogin()
        {
            var result = await StartNetworkConnection().ConfigureAwait(false);

            if (result)
            {
                await DispatcherGameComponent.InvokeAsync(() => _gameStateActions.ChangeToState(GameStates.Login));
            }
        }

        public void ClickViewCredits()
        {
            _gameStateActions.ChangeToState(GameStates.ViewCredits);
        }

        public void ClickExit()
        {
            StopReceivingAndDisconnect();
            _gameStateActions.ExitGame();
        }

        private async Task<bool> StartNetworkConnection()
        {
            if (Interlocked.Increment(ref _numberOfConnectionRequests) != 1)
                return false;

            try
            {
                var connectResult = await _networkConnectionActions.ConnectToServer().ConfigureAwait(false);
                if (connectResult == ConnectResult.AlreadyConnected)
                    return true;

                if (connectResult != ConnectResult.Success)
                {
                    _errorDialogDisplayAction.ShowError(connectResult);
                    return false;
                }

                _backgroundReceiveActions.RunBackgroundReceiveLoop();

                var beginHandshakeOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                    async () => await _networkConnectionActions.BeginHandshake(_random.Next(Constants.MaxChallenge)),
                    ex => _errorDialogDisplayAction.ShowException(ex),
                    ex => _errorDialogDisplayAction.ShowException(ex));

                if (!await beginHandshakeOperation.Invoke().ConfigureAwait(false))
                {
                    StopReceivingAndDisconnect();
                    return false;
                }

                var serverPacket = beginHandshakeOperation.Result;

                if (serverPacket.ReplyCode != InitReply.Ok)
                {
                    _errorDialogDisplayAction.ShowError(serverPacket.ReplyCode, serverPacket.ReplyCodeData);
                    StopReceivingAndDisconnect();
                    return false;
                }

                var okData = (InitInitServerPacket.ReplyCodeDataOk)serverPacket.ReplyCodeData;
                var sequenceStart = InitSequenceStart.FromInitValues(okData.Seq1, okData.Seq2);
                _packetProcessActions.SetSequenceStart(sequenceStart);
                _packetProcessActions.SetEncodeMultiples(okData.ServerEncryptionMultiple, okData.ClientEncryptionMultiple);

                _networkConnectionActions.CompleteHandshake(serverPacket);
                return true;
            }
            finally
            {
                Interlocked.Exchange(ref _numberOfConnectionRequests, 0);
            }
        }

        private void StopReceivingAndDisconnect()
        {
            _backgroundReceiveActions.CancelBackgroundReceiveLoop();
            _networkConnectionActions.DisconnectFromServer();
        }
    }

    public interface IMainButtonController
    {
        void GoToInitialState();

        void GoToInitialStateAndDisconnect(bool showLostConnection = false);

        Task ClickCreateAccount();

        Task ClickLogin();

        void ClickViewCredits();

        void ClickExit();
    }
}
