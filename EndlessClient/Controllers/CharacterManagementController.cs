// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EOLib.Domain.Character;
using EOLib.Net.Communication;
using EOLib.Net.Connection;

namespace EndlessClient.Controllers
{
	public class CharacterManagementController : ICharacterManagementController
	{
		private readonly ICreateCharacterDialogFactory _createCharacterDialogFactory;
		private readonly ICharacterManagementActions _characterManagementActions;
		private readonly IErrorDialogDisplayAction _errorDialogDisplayAction;
		private readonly IBackgroundReceiveActions _backgroundReceiveActions;
		private readonly INetworkConnectionActions _networkConnectionActions;
		private readonly IGameStateActions _gameStateActions;

		public CharacterManagementController(ICreateCharacterDialogFactory createCharacterDialogFactory,
											 ICharacterManagementActions characterManagementActions,
											 IErrorDialogDisplayAction errorDialogDisplayAction,
											 IBackgroundReceiveActions backgroundReceiveActions,
											 INetworkConnectionActions networkConnectionActions,
											 IGameStateActions gameStateActions)
		{
			_createCharacterDialogFactory = createCharacterDialogFactory;
			_characterManagementActions = characterManagementActions;
			_errorDialogDisplayAction = errorDialogDisplayAction;
			_backgroundReceiveActions = backgroundReceiveActions;
			_networkConnectionActions = networkConnectionActions;
			_gameStateActions = gameStateActions;
		}

		public async Task CreateCharacter()
		{
			CharacterReply response;
			try
			{
				response = await _characterManagementActions.RequestCharacterCreation();
			}
			catch (NoDataSentException)
			{
				SetInitialStateAndShowError();
				DisconnectAndStopReceiving();
				return;
			}
			catch (EmptyPacketReceivedException)
			{
				SetInitialStateAndShowError();
				DisconnectAndStopReceiving();
				return;
			}

			if (response != CharacterReply.Ok)
			{
				SetInitialStateAndShowError();
				DisconnectAndStopReceiving();
				return;
			}

			//todo: make this into actions (with response message display)
			var dialog = _createCharacterDialogFactory.BuildCreateCharacterDialog();
			ICharacterCreateParameters parameters;
			try
			{
				parameters = await dialog.Show();
			}
			catch (OperationCanceledException) { return; }

			try
			{
				response = await _characterManagementActions.CreateCharacter(parameters);
			}
			catch (NoDataSentException)
			{
				SetInitialStateAndShowError();
				DisconnectAndStopReceiving();
				return;
			}
			catch (EmptyPacketReceivedException)
			{
				SetInitialStateAndShowError();
				DisconnectAndStopReceiving();
				return;
			}

			if (response != CharacterReply.Ok)
			{
				//todo: move to character management actions
				_errorDialogDisplayAction.ShowCharacterManagementMessage(response);
				return;
			}

			_gameStateActions.RefreshCurrentState();
			_errorDialogDisplayAction.ShowCharacterManagementMessage(CharacterReply.Ok);
		}

		public Task RequestDeleteCharacter()
		{
			//requests delete (first click)
			return Task.FromResult(false);
		}

		public Task ConfirmDeleteCharacter()
		{
			//does actual delete (second click)
			return Task.FromResult(false);
		}

		private void SetInitialStateAndShowError()
		{
			_gameStateActions.ChangeToState(GameStates.Initial);
			_errorDialogDisplayAction.ShowError(ConnectResult.SocketError);
		}

		private void DisconnectAndStopReceiving()
		{
			_backgroundReceiveActions.CancelBackgroundReceiveLoop();
			_networkConnectionActions.DisconnectFromServer();
		}
	}
}