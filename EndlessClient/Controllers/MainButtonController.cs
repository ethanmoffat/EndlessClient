// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Domain;
using EOLib.Domain.Protocol;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.PacketProcessing;

namespace EndlessClient.Controllers
{
    [MappedType(BaseType = typeof(IMainButtonController))]
    public class MainButtonController : IMainButtonController
    {
        private readonly INetworkConnectionActions _networkConnectionActions;
        private readonly IErrorDialogDisplayAction _errorDialogDisplayAction;
        private readonly IPacketProcessActions _packetProcessActions;
        private readonly IBackgroundReceiveActions _backgroundReceiveActions;
        private readonly IGameStateActions _gameStateActions;
        private readonly IAccountDialogDisplayActions _accountDialogDisplayActions;
        private readonly IResetStateAction _resetStateAction;
        private readonly IConnectionStateProvider _connectionStateProvider;
        private readonly ISafeNetworkOperationFactory _networkOperationFactory;

        private int _numberOfConnectionRequests;

        public MainButtonController(INetworkConnectionActions networkConnectionActions,
                                    IErrorDialogDisplayAction errorDialogDisplayAction,
                                    IPacketProcessActions packetProcessActions,
                                    IBackgroundReceiveActions backgroundReceiveActions,
                                    IGameStateActions gameStateActions,
                                    IAccountDialogDisplayActions accountDialogDisplayActions,
                                    IResetStateAction resetStateAction,
                                    IConnectionStateProvider connectionStateProvider,
                                    ISafeNetworkOperationFactory networkOperationFactory)
        {
            _networkConnectionActions = networkConnectionActions;
            _errorDialogDisplayAction = errorDialogDisplayAction;
            _packetProcessActions = packetProcessActions;
            _backgroundReceiveActions = backgroundReceiveActions;
            _gameStateActions = gameStateActions;
            _accountDialogDisplayActions = accountDialogDisplayActions;
            _resetStateAction = resetStateAction;
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

            _resetStateAction.ResetState();
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

                var beginHandshakeOperation = _networkOperationFactory.CreateSafeBlockingOperation(
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

                _packetProcessActions.SetInitialSequenceNumber(initData[InitializationDataKey.SequenceByte1],
                    initData[InitializationDataKey.SequenceByte2]);
                _packetProcessActions.SetEncodeMultiples((byte) initData[InitializationDataKey.ReceiveMultiple],
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

    public interface IMainButtonController
    {
        void GoToInitialState();

        void GoToInitialStateAndDisconnect();

        Task ClickCreateAccount();

        Task ClickLogin();

        void ClickViewCredits();

        void ClickExit();
    }
}
