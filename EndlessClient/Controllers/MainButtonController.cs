// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Domain.Protocol;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.PacketProcessing;

namespace EndlessClient.Controllers
{
    public class MainButtonController : IMainButtonController
    {
        private readonly INetworkConnectionActions _networkConnectionActions;
        private readonly IErrorDialogDisplayAction _errorDialogDisplayAction;
        private readonly IPacketProcessorActions _packetProcessorActions;
        private readonly IBackgroundReceiveActions _backgroundReceiveActions;
        private readonly IGameStateActions _gameStateActions;
        private readonly IAccountDialogDisplayActions _accountDialogDisplayActions;
        private readonly IConnectionStateProvider _connectionStateProvider;
        private readonly ISafeInBandNetworkOperationFactory _networkOperationFactory;

        private int _numberOfConnectionRequests;

        public MainButtonController(INetworkConnectionActions networkConnectionActions,
                                    IErrorDialogDisplayAction errorDialogDisplayAction,
                                    IPacketProcessorActions packetProcessorActions,
                                    IBackgroundReceiveActions backgroundReceiveActions,
                                    IGameStateActions gameStateActions,
                                    IAccountDialogDisplayActions accountDialogDisplayActions,
                                    IConnectionStateProvider connectionStateProvider,
                                    ISafeInBandNetworkOperationFactory networkOperationFactory)
        {
            _networkConnectionActions = networkConnectionActions;
            _errorDialogDisplayAction = errorDialogDisplayAction;
            _packetProcessorActions = packetProcessorActions;
            _backgroundReceiveActions = backgroundReceiveActions;
            _gameStateActions = gameStateActions;
            _accountDialogDisplayActions = accountDialogDisplayActions;
            _connectionStateProvider = connectionStateProvider;
            _networkOperationFactory = networkOperationFactory;
        }

        public void GoToInitialState()
        {
            _gameStateActions.ChangeToState(GameStates.Initial);
        }

        public void GoToInitialStateAndDisconnect()
        {
            GoToInitialState();
            StopReceivingAndDisconnect();
        }

        public async Task ClickCreateAccount()
        {
            var result = await StartNetworkConnection();

            if (result)
            {
                _gameStateActions.ChangeToState(GameStates.CreateAccount);
                _accountDialogDisplayActions.ShowInitialCreateWarningDialog();
            }
        }

        public async Task ClickLogin()
        {
            var result = await StartNetworkConnection();

            if (result)
                _gameStateActions.ChangeToState(GameStates.Login);
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
                var connectResult = await (_connectionStateProvider.NeedsReconnect ? 
                    _networkConnectionActions.ReconnectToServer() :
                    _networkConnectionActions.ConnectToServer());

                if (connectResult == ConnectResult.AlreadyConnected)
                    return true;
                if (connectResult != ConnectResult.Success)
                {
                    _errorDialogDisplayAction.ShowError(connectResult);
                    return false;
                }

                _backgroundReceiveActions.RunBackgroundReceiveLoop();

                var beginHandshakeOperation = _networkOperationFactory.CreateSafeOperation(
                    _networkConnectionActions.BeginHandshake,
                    ex => _errorDialogDisplayAction.ShowException(ex),
                    ex => _errorDialogDisplayAction.ShowException(ex));
                if (!await beginHandshakeOperation.Invoke())
                    return false;
                var initData = beginHandshakeOperation.Result;

                if (initData.Response != InitReply.Success)
                {
                    _errorDialogDisplayAction.ShowError(initData);
                    StopReceivingAndDisconnect();
                    return false;
                }

                _packetProcessorActions.SetInitialSequenceNumber(initData[InitializationDataKey.SequenceByte1],
                    initData[InitializationDataKey.SequenceByte2]);
                _packetProcessorActions.SetEncodeMultiples((byte) initData[InitializationDataKey.ReceiveMultiple],
                    (byte) initData[InitializationDataKey.SendMultiple]);

                _networkConnectionActions.CompleteHandshake(initData);
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
}
