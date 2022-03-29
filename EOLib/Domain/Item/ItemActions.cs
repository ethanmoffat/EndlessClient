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

        public void UseItem(short itemId)
        {
            var packet = new PacketBuilder(PacketFamily.Item, PacketAction.Use)
                .AddShort(itemId)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void EquipItem(short itemId, bool alternateLocation)
        {
            var packet = new PacketBuilder(PacketFamily.PaperDoll, PacketAction.Add)
                .AddShort(itemId)
                .AddChar((byte)(alternateLocation ? 1 : 0))
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void UnequipItem(short itemId, bool alternateLocation)
        {
            var packet = new PacketBuilder(PacketFamily.PaperDoll, PacketAction.Remove)
                .AddShort(itemId)
                .AddChar((byte)(alternateLocation ? 1 : 0))
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void DropItem(short itemId, int amount, MapCoordinate dropPoint)
        {
            var packet = new PacketBuilder(PacketFamily.Item, PacketAction.Drop)
                .AddShort(itemId)
                .AddInt(amount)
                .AddChar((byte)dropPoint.X)
                .AddChar((byte)dropPoint.Y)
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public void JunkItem(IItem item)
        {
            var packet = new PacketBuilder(PacketFamily.Item, PacketAction.Junk)
                .AddShort(item.ItemID)
                .AddInt(item.Amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IItemActions
    {
        void UseItem(short itemId);

        void EquipItem(short itemId, bool alternateLocation);

        void UnequipItem(short itemId, bool alternateLocation);

        void DropItem(short itemId, int amount, MapCoordinate dropPoint);

        void JunkItem(IItem item);
    }
}
