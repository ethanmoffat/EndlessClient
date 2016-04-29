// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Domain.BLL;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using XNAControls;

namespace EndlessClient.Controllers
{
	public class CharacterManagementController : ICharacterManagementController
	{
		private readonly ICharacterManagementActions _characterManagementActions;
		private readonly IErrorDialogDisplayAction _errorDialogDisplayAction;
		private readonly ICharacterDialogActions _characterDialogActions;
		private readonly IBackgroundReceiveActions _backgroundReceiveActions;
		private readonly INetworkConnectionActions _networkConnectionActions;
		private readonly IGameStateActions _gameStateActions;
		private readonly ICharacterSelectorRepository _characterSelectorRepository;

		public CharacterManagementController(ICharacterManagementActions characterManagementActions,
											 IErrorDialogDisplayAction errorDialogDisplayAction,
											 ICharacterDialogActions characterDialogActions,
											 IBackgroundReceiveActions backgroundReceiveActions,
											 INetworkConnectionActions networkConnectionActions,
											 IGameStateActions gameStateActions,
											 ICharacterSelectorRepository characterSelectorRepository)
		{
			_characterManagementActions = characterManagementActions;
			_errorDialogDisplayAction = errorDialogDisplayAction;
			_characterDialogActions = characterDialogActions;
			_backgroundReceiveActions = backgroundReceiveActions;
			_networkConnectionActions = networkConnectionActions;
			_gameStateActions = gameStateActions;
			_characterSelectorRepository = characterSelectorRepository;
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

			var parameters = await _characterDialogActions.ShowCreateCharacterDialog();
			if (parameters == null)
				return;

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

			_gameStateActions.RefreshCurrentState();
			_characterDialogActions.ShowCharacterReplyDialog(response);
		}

		public async Task DeleteCharacter(ICharacter characterToDelete)
		{
			if (_characterSelectorRepository.CharacterForDelete == null ||
			    _characterSelectorRepository.CharacterForDelete != characterToDelete)
			{
				_characterDialogActions.ShowCharacterDeleteWarning(characterToDelete.Name);
				_characterSelectorRepository.CharacterForDelete = characterToDelete;
				return;
			}

			//do TAKE action w/ server

			var dialogResult = await _characterDialogActions.ShowConfirmDeleteWarning();
			if (dialogResult != XNADialogResult.OK)
				return;

			//do DELETE action w/ server
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