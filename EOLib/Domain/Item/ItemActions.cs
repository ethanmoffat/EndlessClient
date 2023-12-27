using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Item
{
    [AutoMappedType]
    public class ItemActions : IItemActions
    {
        private readonly IPacketSendService _packetSendService;

        public ItemActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void UseItem(int itemId)
        {
            var packet = new PacketBuilder(PacketFamily.Item, PacketAction.Use)
                .AddShort(itemId)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void EquipItem(int itemId, bool alternateLocation)
        {
            var packet = new PacketBuilder(PacketFamily.PaperDoll, PacketAction.Add)
                .AddShort(itemId)
                .AddChar(alternateLocation ? 1 : 0)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void UnequipItem(int itemId, bool alternateLocation)
        {
            var packet = new PacketBuilder(PacketFamily.PaperDoll, PacketAction.Remove)
                .AddShort(itemId)
                .AddChar(alternateLocation ? 1 : 0)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void DropItem(int itemId, int amount, MapCoordinate dropPoint)
        {
            var packet = new PacketBuilder(PacketFamily.Item, PacketAction.Drop)
                .AddShort(itemId)
                .AddInt(amount)
                .AddChar(dropPoint.X)
                .AddChar(dropPoint.Y)
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public void JunkItem(int itemId, int amount)
        {
            var packet = new PacketBuilder(PacketFamily.Item, PacketAction.Junk)
                .AddShort(itemId)
                .AddInt(amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IItemActions
    {
        void UseItem(int itemId);

        void EquipItem(int itemId, bool alternateLocation);

        void UnequipItem(int itemId, bool alternateLocation);

        void DropItem(int itemId, int amount, MapCoordinate dropPoint);

        void JunkItem(int itemId, int amount);
    }
}
