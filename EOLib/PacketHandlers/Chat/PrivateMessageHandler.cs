// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Net;

namespace EOLib.PacketHandlers.Chat
{
    public class PrivateMessageHandler : PlayerChatByNameBase
    {
        private readonly IChatRepository _chatRepository;

        public override PacketAction Action { get { return PacketAction.Tell; } }

        public PrivateMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                     IChatRepository chatRepository)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
        }

        protected override void PostChat(string name, string message)
        {
            var localData = new ChatData(name, message, ChatIcon.Note, ChatColor.PM);
            var pmData = new ChatData(name, message, ChatIcon.Note);

            var whichPMTab = _chatRepository.PMTarget1.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                ? ChatTab.Private1
                : _chatRepository.PMTarget2.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    ? ChatTab.Private2
                    : ChatTab.Local;

            _chatRepository.AllChat[ChatTab.Local].Add(localData);
            if (whichPMTab != ChatTab.Local)
                _chatRepository.AllChat[whichPMTab].Add(pmData);
        }
    }
}