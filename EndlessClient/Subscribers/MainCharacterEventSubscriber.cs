using AutomaticTypeMapper;
using EndlessClient.HUD;
using EOLib.Domain.Chat;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Subscribers
{
    [MappedType(BaseType = typeof(IMainCharacterEventNotifier))]
    public class MainCharacterEventSubscriber : IMainCharacterEventNotifier
    {
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IPubFileProvider _pubFileProvider;

        public MainCharacterEventSubscriber(IStatusLabelSetter statusLabelSetter,
                                            IChatRepository chatRepository,
                                            ILocalizedStringFinder localizedStringFinder,
                                            IPubFileProvider pubFileProvider)
        {
            _statusLabelSetter = statusLabelSetter;
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
            _pubFileProvider = pubFileProvider;
        }

        public void NotifyGainedExp(int expDifference)
        {
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION,
                                              EOResourceID.STATUS_LABEL_YOU_GAINED_EXP,
                $"{expDifference} EXP");

            var youGained = _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_YOU_GAINED_EXP);
            var message = $"{youGained} {expDifference} EXP";

            var chatData = new ChatData(string.Empty, message, ChatIcon.Star);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);
        }

        public void NotifyTakeDamage(int damageTaken, int playerPercentHealth)
        {
            //todo: show health bar
        }

        public void TakeItemFromMap(short id, int amountTaken)
        {
            var rec = _pubFileProvider.EIFFile[id];

            var chatMessage = $"{_localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_ITEM_PICKUP_YOU_PICKED_UP)} {amountTaken} {rec.Name}";
            _chatRepository.AllChat[ChatTab.System].Add(new ChatData(string.Empty, chatMessage, ChatIcon.UpArrow));

            _statusLabelSetter.SetStatusLabel(
                EOResourceID.STATUS_LABEL_TYPE_INFORMATION, 
                EOResourceID.STATUS_LABEL_ITEM_PICKUP_YOU_PICKED_UP,
                $" {amountTaken} {rec.Name}");
        }
    }
}
