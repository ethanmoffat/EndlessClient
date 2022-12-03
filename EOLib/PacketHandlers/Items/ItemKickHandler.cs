using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when a quest takes an item from the main character
    /// </summary>
    [AutoMappedType]
    public class ItemKickHandler : QuestItemChangeHandler
    {
        public override PacketAction Action => PacketAction.Kick;

        public ItemKickHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository inventoryRepository)
            : base(playerInfoProvider, characterRepository, inventoryRepository)
        {
        }
    }
}
