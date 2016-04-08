// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Data.Protocol;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.PacketProcessing;

namespace EndlessClient.ControlSets
{
	public class MainButtonController : IMainButtonController
	{
		private readonly INetworkConnectionActions _networkConnectionActions;
		private readonly IErrorDialogDisplayAction _errorDialogDisplayAction;
		private readonly IPacketProcessorActions _packetProcessorActions;
		private readonly IBackgroundReceiveActions _backgroundReceiveActions;
		private readonly IGameStateActions _gameStateActions;
		private readonly IEndlessGame _endlessGame;

		public MainButtonController(INetworkConnectionActions networkConnectionActions,
									IErrorDialogDisplayAction errorDialogDisplayAction,
									IPacketProcessorActions packetProcessorActions,
									IBackgroundReceiveActions backgroundReceiveActions,
									IGameStateActions gameStateActions,
									IEndlessGame endlessGame)
		{
			_networkConnectionActions = networkConnectionActions;
			_errorDialogDisplayAction = errorDialogDisplayAction;
			_packetProcessorActions = packetProcessorActions;
			_backgroundReceiveActions = backgroundReceiveActions;
			_gameStateActions = gameStateActions;
			_endlessGame = endlessGame;
		}

		public void GoToInitialState()
		{
			
		}

		public async Task ClickCreateAccount()
		{
			await StartNetworkConnection();

			_gameStateActions.ChangeToState(GameStates.CreateAccount);
		}

		public async Task ClickLogin()
		{
			await StartNetworkConnection();

			_gameStateActions.ChangeToState(GameStates.Login);
		}

		public void ClickViewCredits()
		{
			_gameStateActions.ChangeToState(GameStates.ViewCredits);
		}

		public void ClickExit()
		{
			_backgroundReceiveActions.CancelBackgroundReceiveLoop();
			_networkConnectionActions.DisconnectFromServer();

			_endlessGame.Exit();
		}

		private async Task StartNetworkConnection()
		{
			var connectResult = await _networkConnectionActions.ConnectToServer();
			if (connectResult != ConnectResult.Success)
			{
				_errorDialogDisplayAction.ShowError(connectResult);
				return;
			}

			_backgroundReceiveActions.RunBackgroundReceiveLoop();

			var initData = await _networkConnectionActions.BeginHandshake();
			if (initData.Response != InitReply.Success)
			{
				_errorDialogDisplayAction.ShowError(initData);
				return;
			}

			_packetProcessorActions.SetInitialSequenceNumber(initData[InitializationDataKey.SequenceByte1],
															 initData[InitializationDataKey.SequenceByte2]);
			_packetProcessorActions.SetEncodeMultiples((byte)initData[InitializationDataKey.ReceiveMultiple],
													   (byte)initData[InitializationDataKey.SendMultiple]);

			_networkConnectionActions.CompleteHandshake(initData);
		}
	}
}
