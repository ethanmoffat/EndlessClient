using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.Localization;

namespace EndlessClient.Subscribers
{
    [AutoMappedType]
    public class DoorEventSubscriber : IDoorEventNotifier
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IStatusLabelSetter _statusLabelSetter;

        public DoorEventSubscriber(IEOMessageBoxFactory messageBoxFactory,
                                    IStatusLabelSetter statusLabelSetter,
                                    ISfxPlayer sfxPlayer)
        {
            _messageBoxFactory = messageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
            _sfxPlayer = sfxPlayer;
        }

        public void NotifyDoorLocked(ChestKey key)
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.DOOR_LOCKED);
            dlg.ShowDialog();

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                EOResourceID.STATUS_LABEL_THE_DOOR_IS_LOCKED_EXCLAMATION,
                $" - {key}");

            _sfxPlayer.PlaySfx(SoundEffectID.DoorOrChestLocked);
        }
    }
}
