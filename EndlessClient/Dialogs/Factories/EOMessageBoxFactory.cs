using System.Text;
using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EOLib.Graphics;
using EOLib.Localization;
using Optional;
using XNAControls;

namespace EndlessClient.Dialogs.Factories
{
    [MappedType(BaseType = typeof(IEOMessageBoxFactory))]
    public class EOMessageBoxFactory : IEOMessageBoxFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IActiveDialogRepository _activeDialogRepository;
        private readonly ISfxPlayer _sfxPlayer;

        public EOMessageBoxFactory(INativeGraphicsManager nativeGraphicsManager,
                                   IGameStateProvider gameStateProvider,
                                   IEODialogButtonService eoDialogButtonService,
                                   ILocalizedStringFinder localizedStringFinder,
                                   IActiveDialogRepository activeDialogRepository,
                                   ISfxPlayer sfxPlayer)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _eoDialogButtonService = eoDialogButtonService;
            _localizedStringFinder = localizedStringFinder;
            _activeDialogRepository = activeDialogRepository;
            _sfxPlayer = sfxPlayer;
        }

        public IXNADialog CreateMessageBox(string message,
                                           string caption = "",
                                           EODialogButtons whichButtons = EODialogButtons.Ok,
                                           EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader)
        {

            var messageBox = new EOMessageBox(_nativeGraphicsManager,
                                              _gameStateProvider,
                                              _eoDialogButtonService,
                                              message,
                                              caption,
                                              style,
                                              whichButtons);
            messageBox.DialogClosing += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);
            messageBox.DialogClosed += (_, _) => _activeDialogRepository.MessageBox = Option.None<EOMessageBox>();

            _activeDialogRepository.MessageBox = Option.Some(messageBox);

            return messageBox;
        }

        public IXNADialog CreateMessageBox(DialogResourceID resource,
                                           EODialogButtons whichButtons = EODialogButtons.Ok,
                                           EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader)
        {
            return CreateMessageBox(_localizedStringFinder.GetString(resource + 1),
                _localizedStringFinder.GetString(resource),
                whichButtons,
                style);
        }

        public IXNADialog CreateMessageBox(string prependData,
                                           DialogResourceID resource,
                                           EODialogButtons whichButtons = EODialogButtons.Ok,
                                           EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader)
        {
            var message = prependData + _localizedStringFinder.GetString(resource + 1);
            return CreateMessageBox(message,
                _localizedStringFinder.GetString(resource),
                whichButtons,
                style);
        }

        public IXNADialog CreateMessageBox(DialogResourceID resource,
                                           string extraData,
                                           EODialogButtons whichButtons = EODialogButtons.Ok,
                                           EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader)
        {
            var message = _localizedStringFinder.GetString(resource + 1) + extraData;
            return CreateMessageBox(message,
                _localizedStringFinder.GetString(resource),
                whichButtons,
                style);
        }

        public IXNADialog CreateMessageBox(string prependData,
                                           DialogResourceID resource,
                                           EODialogButtons whichButtons = EODialogButtons.Ok,
                                           EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader,
                                           params EOResourceID[] appendResources)
        {
            var message = new StringBuilder(prependData);
            message.Append(_localizedStringFinder.GetString(resource + 1));
            foreach (var resourceId in appendResources)
                message.Append(" " + _localizedStringFinder.GetString(resourceId));
            return CreateMessageBox(message.ToString(),
                _localizedStringFinder.GetString(resource),
                whichButtons,
                style);
        }

        public IXNADialog CreateMessageBox(EOResourceID message,
                                           EOResourceID caption,
                                           EODialogButtons whichButtons = EODialogButtons.Ok,
                                           EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader)
        {
            var messageText = _localizedStringFinder.GetString(message);
            var captionText = _localizedStringFinder.GetString(caption);
            return CreateMessageBox(messageText, captionText, whichButtons, style);
        }
    }
}
