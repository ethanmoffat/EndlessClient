﻿using System.Linq;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chat
{
    public abstract class PlayerChatByIDHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        public override PacketFamily Family => PacketFamily.Talk;

        protected PlayerChatByIDHandler(ICurrentMapStateProvider currentMapStateProvider,
                                        IPlayerInfoProvider playerInfoProvider)
            : base(playerInfoProvider)
        {
            _currentMapStateProvider = currentMapStateProvider;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var fromPlayerID = packet.ReadShort();
            if (_currentMapStateProvider.Characters.All(x => x.ID != fromPlayerID))
                return true;

            DoTalk(packet, _currentMapStateProvider.Characters.Single(x => x.ID == fromPlayerID));

            return true;
        }

        protected abstract void DoTalk(IPacket packet, ICharacter character);
    }
}
