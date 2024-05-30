using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chest
{
    /// <summary>
    /// Handler for CHEST_GET packet, sent as confirmation to character that item is being taken
    /// </summary>
    [AutoMappedType]
    public class ChestGetHandler : ChestItemUpdateHandler<ChestGetServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Chest;

        public override PacketAction Action => PacketAction.Get;

        public ChestGetHandler(IPlayerInfoProvider playerInfoProvider,
                               IChestDataRepository chestDataRepository,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, chestDataRepository, characterRepository, characterInventoryRepository)
        {
        }

        public override bool HandlePacket(ChestGetServerPacket packet)
        {
            Handle(packet.Items, packet.TakenItem, packet.Weight, addingItemFromInventory: false);
            return true;
        }
    }
}
