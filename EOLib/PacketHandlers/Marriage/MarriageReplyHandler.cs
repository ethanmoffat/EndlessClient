using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Marriage
{
    [AutoMappedType]
    public class MarriageReplyHandler : InGameOnlyPacketHandler<MarriageReplyServerPacket>
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Marriage;

        public override PacketAction Action => PacketAction.Reply;

        public MarriageReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICharacterInventoryRepository characterInventoryRepository,
                                    IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(MarriageReplyServerPacket packet)
        {
            if (packet.ReplyCode == MarriageReply.Success)
            {
                var data = (MarriageReplyServerPacket.ReplyCodeDataSuccess)packet.ReplyCodeData;
                _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
                _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, data.GoldAmount));
            }

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyMarriageReply(packet.ReplyCode);

            return true;
        }
    }
}
