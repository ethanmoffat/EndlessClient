using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class PlayerEmoteHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IEmoteNotifier> _emoteNotifiers;

        public override PacketFamily Family => PacketFamily.Emote;

        public override PacketAction Action => PacketAction.Player;

        public PlayerEmoteHandler(IPlayerInfoProvider playerInfoProvider,
                                  IEnumerable<IEmoteNotifier> emoteNotifiers)
            : base(playerInfoProvider)
        {
            _emoteNotifiers = emoteNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerId = packet.ReadShort();
            var emote = (Emote)packet.ReadChar();
            foreach (var notifier in _emoteNotifiers)
                notifier.NotifyEmote(playerId, emote);

            return true;
        }
    }
}
