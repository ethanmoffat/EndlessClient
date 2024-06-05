using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Notifiers;
using EOLib.Localization;

namespace EndlessClient.Subscribers
{
    [AutoMappedType]
    public class LockerEventSubscriber : ILockerEventNotifier
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public LockerEventSubscriber(IEOMessageBoxFactory messageBoxFactory,
                                     ILocalizedStringFinder localizedStringFinder)
        {
            _messageBoxFactory = messageBoxFactory;
            _localizedStringFinder = localizedStringFinder;
        }

        public void NotifyLockerFull(int maxItems)
        {
            var message = _localizedStringFinder.GetString(DialogResourceID.LOCKER_FULL_DIFF_ITEMS_MAX + 1);
            var caption = _localizedStringFinder.GetString(DialogResourceID.LOCKER_FULL_DIFF_ITEMS_MAX);

            var dlg = _messageBoxFactory.CreateMessageBox(message.Replace("25", $"{maxItems}"), caption);
            dlg.ShowDialog();
        }
    }
}
