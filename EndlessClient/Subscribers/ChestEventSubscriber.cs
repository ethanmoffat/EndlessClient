using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EndlessClient.Rendering;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.Localization;

namespace EndlessClient.Subscribers
{
    [AutoMappedType]
    public class ChestEventSubscriber : IChestEventNotifier
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IActiveDialogRepository _activeDialogRepository;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IUnlockChestValidator _unlockChestValidator;
        private readonly IStatusLabelSetter _statusLabelSetter;

        public ChestEventSubscriber(IEOMessageBoxFactory messageBoxFactory,
                                    IActiveDialogRepository activeDialogRepository,
                                    IUnlockChestValidator unlockChestValidator,
                                    IStatusLabelSetter statusLabelSetter,
                                    ISfxPlayer sfxPlayer)
        {
            _messageBoxFactory = messageBoxFactory;
            _activeDialogRepository = activeDialogRepository;
            _unlockChestValidator = unlockChestValidator;
            _statusLabelSetter = statusLabelSetter;
            _sfxPlayer = sfxPlayer;
        }

        public void NotifyChestBroken()
        {
            DispatcherGameComponent.Invoke(() => _activeDialogRepository.ChestDialog.MatchSome(x => x.Close()));

            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.CHEST_BROKEN);
            dlg.ShowDialog();

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_THE_CHEST_SEEMS_BROKEN);
        }

        public void NotifyChestLocked(ChestKey key)
        {
            DispatcherGameComponent.Invoke(() => _activeDialogRepository.ChestDialog.MatchSome(x => x.Close()));

            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.CHEST_LOCKED);
            dlg.ShowDialog();

            var requiredKey = _unlockChestValidator.GetRequiredKeyName(key);
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_THE_CHEST_IS_LOCKED_EXCLAMATION, requiredKey.Match(x => $" - {x}", () => string.Empty));

            _sfxPlayer.PlaySfx(SoundEffectID.DoorOrChestLocked);
        }
    }
}
