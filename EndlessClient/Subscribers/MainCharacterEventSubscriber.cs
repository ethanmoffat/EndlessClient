// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.HUD;
using EOLib.Domain.Chat;
using EOLib.Domain.Notifiers;
using EOLib.Localization;

namespace EndlessClient.Subscribers
{
    public class MainCharacterEventSubscriber : IMainCharacterEventNotifier
    {
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public MainCharacterEventSubscriber(IStatusLabelSetter statusLabelSetter,
                                            IChatRepository chatRepository,
                                            ILocalizedStringFinder localizedStringFinder)
        {
            _statusLabelSetter = statusLabelSetter;
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
        }

        public void NotifyGainedExp(int expDifference)
        {
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION,
                                              EOResourceID.STATUS_LABEL_YOU_GAINED_EXP,
                                              string.Format("{0} EXP", expDifference));

            var youGained = _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_YOU_GAINED_EXP);
            var message = string.Format("{0} {1} EXP", youGained, expDifference);

            var chatData = new ChatData(string.Empty, message, ChatIcon.Star);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);
        }
    }
}
