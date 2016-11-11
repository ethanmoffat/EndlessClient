// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chat
{
    public class MuteHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketFamily Family { get { return PacketFamily.Talk; } }

        public override PacketAction Action { get { return PacketAction.Spec; } }

        public MuteHandler(IPlayerInfoProvider playerInfoProvider,
                           IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var adminName = packet.ReadEndString();
            adminName = char.ToUpper(adminName[0]) + adminName.Substring(1).ToLower();

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyPlayerMutedByAdmin(adminName);

            return true;
        }
    }
}
