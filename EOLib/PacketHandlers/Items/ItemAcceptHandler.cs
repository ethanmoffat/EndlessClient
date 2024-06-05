using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when another player uses an EXP scroll type item
    /// </summary>
    [AutoMappedType]
    public class ItemAcceptHandler : InGameOnlyPacketHandler<ItemAcceptServerPacket>
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

        public override bool HandlePacket(ItemAcceptServerPacket packet)
        {
            foreach (var notifier in _emoteNotifiers)
                notifier.NotifyEmote(packet.PlayerId, Domain.Character.Emote.LevelUp);

            return true;
        }
    }
}
