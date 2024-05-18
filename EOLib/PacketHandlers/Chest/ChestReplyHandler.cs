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
    public class ChestReplyHandler : ChestGetHandler
    {
        public override PacketAction Action => PacketAction.Reply;

        public ChestReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                 IChestDataRepository chestDataRepository,
                                 ICharacterRepository characterRepository,
                                 ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, chestDataRepository, characterRepository, characterInventoryRepository)
        {
        }

        public override bool HandlePacket(ChestGetServerPacket packet)
        {
            Handle(packet.Items, packet.TakenItem, packet.Weight, addingItemFromInventory: true);
            return true;
        }
    }
}
