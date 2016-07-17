// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net;
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
            short createID;
            try
            {
                createID = await _characterManagementActions.RequestCharacterCreation();
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
            
            //todo: other server implementations might have a different value than 1000
            //        it is set to a constant 1000 in eoserv
            if (createID != 1000)
            {
                SetInitialStateAndShowError();
                DisconnectAndStopReceiving();
                return;
            }

            //todo: make not approved character names cancel the dialog close
            var parameters = await _characterDialogActions.ShowCreateCharacterDialog();
            if (parameters == null)
                return;

            CharacterReply response;
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

            if (response == CharacterReply.Ok)
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

            int takeID;
            try
            {
                takeID = await _characterManagementActions.RequestCharacterDelete();
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

            if (takeID != characterToDelete.ID)
            {
                _characterDialogActions.ShowCharacterDeleteError();
                return;
            }

            var dialogResult = await _characterDialogActions.ShowConfirmDeleteWarning(characterToDelete.Name);
            if (dialogResult != XNADialogResult.OK)
                return;

            CharacterReply response;
            try
            {
                response = await _characterManagementActions.DeleteCharacter();
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

            _characterSelectorRepository.CharacterForDelete = null;
            if (response != CharacterReply.Deleted)
            {
                SetInitialStateAndShowError();
                DisconnectAndStopReceiving();
                return;
            }
            
            _gameStateActions.RefreshCurrentState();
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