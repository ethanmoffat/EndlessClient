using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chest
{
    /// <summary>
    /// Handler for CHEST_REPLY packet, sent in response to main player adding an item to a chest
    /// </summary>
    [AutoMappedType]
    public class ChestReplyHandler : ChestItemUpdateHandler<ChestReplyServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Chest;

        public override PacketAction Action => PacketAction.Reply;

        public ChestReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                 IChestDataRepository chestDataRepository,
                                 ICharacterRepository characterRepository,
                                 ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, chestDataRepository, characterRepository, characterInventoryRepository)
        {
        }

        public override bool HandlePacket(ChestReplyServerPacket packet)
        {
            var item = new ThreeItem
            {
                Id = packet.AddedItemId,
                Amount = packet.RemainingAmount,
            };
            Handle(packet.Items, item, packet.Weight, addingItemFromInventory: true);
            return true;
        }
    }
}
