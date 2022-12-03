using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when another player uses an EXP scroll type item
    /// </summary>
    [AutoMappedType]
    public class ItemAcceptHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IEmoteNotifier> _emoteNotifiers;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Accept;

        public ItemAcceptHandler(IPlayerInfoProvider playerInfoProvider,
                                 IEnumerable<IEmoteNotifier> emoteNotifiers)
            : base(playerInfoProvider)
        {
            _emoteNotifiers = emoteNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerId = packet.ReadShort();
            foreach (var notifier in _emoteNotifiers)
                notifier.NotifyEmote(playerId, Domain.Character.Emote.LevelUp);

            return true;
        }
    }
}
