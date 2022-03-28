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

        void JunkItem(IItem item);
    }
}
