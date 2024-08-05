using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Items
{
    [AutoMappedType]
    public class ItemAgreeHandler : InGameOnlyPacketHandler<ItemAgreeServerPacket>
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Agree;

        public ItemAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(ItemAgreeServerPacket packet)
        {
            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == packet.ItemId);
            return true;
        }
    }
}