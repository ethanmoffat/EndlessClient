using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when a quest gives an item to the main character
    /// </summary>
    [AutoMappedType]
    public class ItemObtainHandler : QuestItemChangeHandler
    {
        public override PacketAction Action => PacketAction.Obtain;

        public ItemObtainHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterRepository characterRepository,
                                 ICharacterInventoryRepository inventoryRepository)
            : base(playerInfoProvider, characterRepository, inventoryRepository)
        {
        }
    }
}
