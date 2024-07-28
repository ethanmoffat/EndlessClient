using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when a quest takes an item from the main character
    /// </summary>
    [AutoMappedType]
    public class ItemKickHandler : QuestItemChangeHandler<ItemKickServerPacket>
    {
        public override PacketAction Action => PacketAction.Kick;

        public ItemKickHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository inventoryRepository)
            : base(playerInfoProvider, characterRepository, inventoryRepository)
        {
        }

        public override bool HandlePacket(ItemKickServerPacket packet)
        {
            Handle(packet.Item.Id, packet.Item.Amount, packet.CurrentWeight);
            return true;
        }
    }
}