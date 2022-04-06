using AutomaticTypeMapper;
using EOLib.Domain.NPC;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact
{
    [AutoMappedType]
    public class MapNPCActions : IMapNPCActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IENFFileProvider _enfFileProvider;

        public MapNPCActions(IPacketSendService packetSendService,
                             IENFFileProvider enfFileProvider)
        {
            _packetSendService = packetSendService;
            _enfFileProvider = enfFileProvider;
        }

        public void RequestShop(INPC npc)
        {
            var packet = new PacketBuilder(PacketFamily.Shop, PacketAction.Open)
                .AddShort(npc.Index)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void RequestQuest(INPC npc)
        {
            var data = _enfFileProvider.ENFFile[npc.ID];

            var packet = new PacketBuilder(PacketFamily.Quest, PacketAction.Use)
                .AddShort(npc.Index)
                .AddShort(data.VendorID)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IMapNPCActions
    {
        void RequestShop(INPC npc);

        void RequestQuest(INPC npc);
    }
}
