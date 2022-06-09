using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.Audio;
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
        private readonly ISfxPlayer _sfxPlayer;

        public CharacterManagementController(ISafeNetworkOperationFactory safeNetworkOperationFactory,
                                             ICharacterManagementActions characterManagementActions,
                                             IErrorDialogDisplayAction errorDialogDisplayAction,
                                             ICharacterDialogActions characterDialogActions,
                                             IBackgroundReceiveActions backgroundReceiveActions,
                                             INetworkConnectionActions networkConnectionActions,
                                             IGameStateActions gameStateActions,
                                             ICharacterSelectorRepository characterSelectorRepository,
                                             ISfxPlayer sfxPlayer)
        {
            _safeNetworkOperationFactory = safeNetworkOperationFactory;
            _characterManagementActions = characterManagementActions;
            _errorDialogDisplayAction = errorDialogDisplayAction;
            _characterDialogActions = characterDialogActions;
            _backgroundReceiveActions = backgroundReceiveActions;
            _networkConnectionActions = networkConnectionActions;
            _gameStateActions = gameStateActions;
            _characterSelectorRepository = characterSelectorRepository;
            _sfxPlayer = sfxPlayer;
        }

        public async Task CreateCharacter()
        {
            var requestCreateOp = _safeNetworkOperationFactory.CreateSafeBlockingOperation(_characterManagementActions.RequestCharacterCreation, SendError, RecvError);
            if (!await requestCreateOp.Invoke().ConfigureAwait(false))
                return;

            var createID = requestCreateOp.Result;

            //todo: make not approved character names cancel the dialog close
            var showResult = await _characterDialogActions.ShowCreateCharacterDialog().ConfigureAwait(false);
            showResult.MatchSome(parameters =>
            {
                var createOp = _safeNetworkOperationFactory.CreateSafeBlockingOperation(
                    () => _characterManagementActions.CreateCharacter(parameters, createID), SendError, RecvError);
                var opTask = createOp.Invoke();
                opTask.ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        _gameStateActions.RefreshCurrentState();
                        _characterDialogActions.ShowCharacterReplyDialog(createOp.Result);
                    }
                });
            });
        }

        public async Task DeleteCharacter(Character characterToDelete)
        {
            void ShowCharacterDeleteWarning(Character c)
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
            if (!await requestDeleteOp.Invoke().ConfigureAwait(false))
                return;

            var takeID = requestDeleteOp.Result;

            var dialogResult = await _characterDialogActions.ShowConfirmDeleteWarning(characterToDelete.Name).ConfigureAwait(false);
            if (dialogResult != XNADialogResult.OK)
                return;

            var deleteOp = _safeNetworkOperationFactory.CreateSafeBlockingOperation(() => _characterManagementActions.DeleteCharacter(takeID), SendError, RecvError);
            if (!await deleteOp.Invoke().ConfigureAwait(false))
                return;

            _characterSelectorRepository.CharacterForDelete = Option.None<Character>();
            if (deleteOp.Result != CharacterReply.Deleted)
            {
                SetInitialStateAndShowError();
                DisconnectAndStopReceiving();
                return;
            }

            _sfxPlayer.PlaySfx(SoundEffectID.DeleteCharacter);
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

        Task DeleteCharacter(Character characterToDelete);
    }
}