// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EOLib;
using EOLib.Domain.Chat;

namespace EndlessClient.HUD.Chat
{
    public class ChatNotificationActions : IChatEventNotifier
    {
        private readonly IChatRepository _chatRepository;
        private readonly IHudControlProvider _hudControlProvider;

        public ChatNotificationActions(IChatRepository chatRepository,
                                       IHudControlProvider hudControlProvider)
        {
            _chatRepository = chatRepository;
            _hudControlProvider = hudControlProvider;
        }

        public void NotifyPrivateMessageRecipientNotFound(string recipientName)
        {
            var whichTab = _chatRepository.PMTarget1.ToLower() == recipientName.ToLower()
                ? new Optional<ChatTab>(ChatTab.Private1)
                : _chatRepository.PMTarget2.ToLower() == recipientName.ToLower()
                    ? new Optional<ChatTab>(ChatTab.Private2)
                    : Optional<ChatTab>.Empty;

            if (whichTab.HasValue)
            {
                if (whichTab == ChatTab.Private1)
                    _chatRepository.PMTarget1 = string.Empty;
                else if (whichTab == ChatTab.Private2)
                    _chatRepository.PMTarget2 = string.Empty;

                _chatRepository.AllChat[whichTab].Clear();

                var chatPanel = _hudControlProvider.GetComponent<ChatPanel>(HudControlIdentifier.ChatPanel);
                chatPanel.ClosePMTab(whichTab);
            }
        }

        public void NotifyPlayerMutedByAdmin(string adminName)
        {
            //todo
        }
    }
}
