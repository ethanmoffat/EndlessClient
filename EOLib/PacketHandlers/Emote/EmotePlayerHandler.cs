using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Emote
{
    /// <summary>
    /// Sent when a player does an emote
    /// </summary>
    [AutoMappedType]
    public class EmotePlayerHandler : InGameOnlyPacketHandler<EmotePlayerServerPacket>
    {
        private readonly IEnumerable<IEmoteNotifier> _emoteNotifiers;

        public override PacketFamily Family => PacketFamily.Emote;

        public override PacketAction Action => PacketAction.Player;

        public EmotePlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                  IEnumerable<IEmoteNotifier> emoteNotifiers)
            : base(playerInfoProvider)
        {
            _emoteNotifiers = emoteNotifiers;
        }

        public override bool HandlePacket(EmotePlayerServerPacket packet)
        {
            foreach (var notifier in _emoteNotifiers)
                notifier.NotifyEmote(packet.PlayerId, (Domain.Character.Emote)packet.Emote);

            return true;
        }
    }
}
