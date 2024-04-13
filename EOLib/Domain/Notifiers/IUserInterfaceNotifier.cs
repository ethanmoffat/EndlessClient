using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Net;
using System.Collections.Generic;

namespace EOLib.Domain.Notifiers
{
    public interface IUserInterfaceNotifier
    {
        void NotifyPacketDialog(PacketFamily packetFamily);

        void NotifyMessageDialog(string title, IReadOnlyList<string> messages);

        void NotifyCharacterInfo(string name, int mapId, MapCoordinate mapCoords, CharacterStats stats);

        void NotifyCharacterInventory(string name, int usage, int gold, IReadOnlyList<InventoryItem> inventory, IReadOnlyList<InventoryItem> bank);
    }

    [AutoMappedType]
    public class NoOpUserInterfaceNotifier : IUserInterfaceNotifier
    {
        public void NotifyPacketDialog(PacketFamily packetFamily) { }

        public void NotifyMessageDialog(string title, IReadOnlyList<string> messages) { }

        public void NotifyCharacterInfo(string name, int mapId, MapCoordinate mapCoords, CharacterStats stats) { }

        public void NotifyCharacterInventory(string name, int usage, int gold, IReadOnlyList<InventoryItem> inventory, IReadOnlyList<InventoryItem> bank) { }
    }
}
