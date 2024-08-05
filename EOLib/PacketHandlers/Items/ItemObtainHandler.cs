using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when a quest gives an item to the main character
    /// </summary>
    [AutoMappedType]
    public class ItemObtainHandler : QuestItemChangeHandler<ItemObtainServerPacket>
    {
        public override PacketAction Action => PacketAction.Obtain;

        public ItemObtainHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterRepository characterRepository,
                                 ICharacterInventoryRepository inventoryRepository)
            : base(playerInfoProvider, characterRepository, inventoryRepository)
        {
        }

        public override bool HandlePacket(ItemObtainServerPacket packet)
        {
            Handle(packet.Item.Id, packet.Item.Amount, packet.CurrentWeight);
            return true;
        }
    }
}