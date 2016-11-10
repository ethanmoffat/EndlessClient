// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chat
{
    public class MuteHandler : InGameOnlyPacketHandler
    {
        private readonly ChatEventManager _chatEventManager;

        public override PacketFamily Family { get { return PacketFamily.Talk; } }

        public override PacketAction Action { get { return PacketAction.Spec; } }

        public MuteHandler(IPlayerInfoProvider playerInfoProvider,
                           ChatEventManager chatEventManager)
            : base(playerInfoProvider)
        {
            _chatEventManager = chatEventManager;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var adminName = packet.ReadEndString();
            adminName = char.ToUpper(adminName[0]) + adminName.Substring(1).ToLower();

            _chatEventManager.FirePlayerMuted(adminName);

            return true;
        }
    }
}
