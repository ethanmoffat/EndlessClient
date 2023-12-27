using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Law;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Marriage
{
    [AutoMappedType]
    public class MarriageReplyHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var replyCode = (MarriageReply)packet.ReadShort();

            if (replyCode == MarriageReply.Success)
            {
                var goldAmount = packet.ReadInt();

                _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
                _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, goldAmount));
            }

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyMarriageReply(replyCode);

            return true;
        }
    }
}
