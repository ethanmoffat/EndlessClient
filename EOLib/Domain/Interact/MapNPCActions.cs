using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact
{
    [AutoMappedType]
    public class MapNPCActions : IMapNPCActions
    {
        private readonly IPacketSendService _packetSendService;

        public MapNPCActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void RequestShop(byte index)
        {
            var packet = new PacketBuilder(PacketFamily.Shop, PacketAction.Open)
                .AddShort(index)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void RequestQuest(short index, short vendorId)
        {
            var packet = new PacketBuilder(PacketFamily.Quest, PacketAction.Use)
                .AddShort(index)
                .AddShort(vendorId)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IMapNPCActions
    {
        void RequestShop(byte index);

        void RequestQuest(short index, short vendorId);
    }
}
