using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using Optional;
using XNAControls;

namespace EndlessClient.Controllers
{
    [MappedType(BaseType = typeof(ICharacterManagementController))]
    public class CharacterManagementController : ICharacterManagementController
    {
        private readonly ISafeNetworkOperationFactory _safeNetworkOperationFactory;
        private readonly ICharacterManagementActions _characterManagementActions;
        private readonly IErrorDialogDisplayAction _errorDialogDisplayAction;
        private readonly ICharacterDialogActions _characterDialogActions;
        private readonly IBackgroundReceiveActions _backgroundReceiveActions;
        private readonly INetworkConnectionActions _networkConnectionActions;
        private readonly IGameStateActions _gameStateActions;
        private readonly ICharacterSelectorRepository _characterSelectorRepository;

        public CharacterManagementController(ISafeNetworkOperationFactory safeNetworkOperationFactory,
                                             ICharacterManagementActions characterManagementActions,
                                             IErrorDialogDisplayAction errorDialogDisplayAction,
                                             ICharacterDialogActions characterDialogActions,
                                             IBackgroundReceiveActions backgroundReceiveActions,
                                             INetworkConnectionActions networkConnectionActions,
                                             IGameStateActions gameStateActions,
                                             ICharacterSelectorRepository characterSelectorRepository)
        {
            _safeNetworkOperationFactory = safeNetworkOperationFactory;
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
            var requestCreateOp = _safeNetworkOperationFactory.CreateSafeBlockingOperation(_characterManagementActions.RequestCharacterCreation, SendError, RecvError);
            if (!await requestCreateOp.Invoke())
                return;

            var createID = requestCreateOp.Result;

            //todo: make not approved character names cancel the dialog close
            var showResult = await _characterDialogActions.ShowCreateCharacterDialog();
            showResult.MatchSome(async parameters =>
            {
                var createOp = _safeNetworkOperationFactory.CreateSafeBlockingOperation(
                    () => _characterManagementActions.CreateCharacter(parameters, createID), SendError, RecvError);
                if (!await createOp.Invoke())
                    return;

                if (createOp.Result == CharacterReply.Ok)
                    _gameStateActions.RefreshCurrentState();
                _characterDialogActions.ShowCharacterReplyDialog(createOp.Result);
            });
        }

        public async Task DeleteCharacter(ICharacter characterToDelete)
        {
            void ShowCharacterDeleteWarning(ICharacter c)
            {
                _characterDialogActions.ShowCharacterDeleteWarning(c.Name);
                _characterSelectorRepository.CharacterForDelete = Option.Some(c);
            }

            var warningShown = _characterSelectorRepository.CharacterForDelete.Match(
                some: c =>
                {
                    if (c != characterToDelete)
                    {
                        ShowCharacterDeleteWarning(characterToDelete);
                        return true;
                    }

                    return false;
                },
                none: () =>
                {
                    ShowCharacterDeleteWarning(characterToDelete);
                    return true;
                });

            if (warningShown)
                return;

            var requestDeleteOp = _safeNetworkOperationFactory.CreateSafeBlockingOperation(_characterManagementActions.RequestCharacterDelete, SendError, RecvError);
            if (!await requestDeleteOp.Invoke())
                return;

            var takeID = requestDeleteOp.Result;

            var dialogResult = await _characterDialogActions.ShowConfirmDeleteWarning(characterToDelete.Name);
            if (dialogResult != XNADialogResult.OK)
                return;

            var deleteOp = _safeNetworkOperationFactory.CreateSafeBlockingOperation(() => _characterManagementActions.DeleteCharacter(takeID), SendError, RecvError);
            if (!await deleteOp.Invoke())
                return;

            var response = deleteOp.Result;

            _characterSelectorRepository.CharacterForDelete = Option.None<ICharacter>();
            if (response != CharacterReply.Deleted)
            {
                SetInitialStateAndShowError();
                DisconnectAndStopReceiving();
                return;
            }
            
            _gameStateActions.RefreshCurrentState();
        }

        private void SendError(NoDataSentException ndes)
        {
            SetInitialStateAndShowError();
            DisconnectAndStopReceiving();
        }

        private void RecvError(EmptyPacketReceivedException epre)
        {
            SetInitialStateAndShowError();
            DisconnectAndStopReceiving();
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

    public interface ICharacterManagementController
    {
        Task CreateCharacter();

        Task DeleteCharacter(ICharacter characterToDelete);
    }
}