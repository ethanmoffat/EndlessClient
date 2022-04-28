using AutomaticTypeMapper;
using EOLib.Domain.Interact.Quest;
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
        private readonly IQuestDataRepository _questDataRepository;

        public MapNPCActions(IPacketSendService packetSendService,
                             IENFFileProvider enfFileProvider,
                             IQuestDataRepository questDataRepository)
        {
            _packetSendService = packetSendService;
            _enfFileProvider = enfFileProvider;
            _questDataRepository = questDataRepository;
        }

        public void RequestShop(NPC.NPC npc)
        {
            var packet = new PacketBuilder(PacketFamily.Shop, PacketAction.Open)
                .AddShort((short)npc.Index)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void RequestQuest(NPC.NPC npc)
        {
            _questDataRepository.RequestedNPC = npc;

            var data = _enfFileProvider.ENFFile[npc.ID];

            var packet = new PacketBuilder(PacketFamily.Quest, PacketAction.Use)
                .AddShort((short)npc.Index)
                .AddShort(data.VendorID)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void RequestBank(NPC.NPC npc)
        {
            var packet = new PacketBuilder(PacketFamily.Bank, PacketAction.Open)
                .AddShort((short)npc.Index)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void RequestSkillmaster(NPC.NPC npc)
        {
            var packet = new PacketBuilder(PacketFamily.StatSkill, PacketAction.Open)
                .AddShort((short)npc.Index)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IMapNPCActions
    {
        void RequestShop(NPC.NPC npc);

        void RequestQuest(NPC.NPC npc);

        void RequestBank(NPC.NPC npc);

        void RequestSkillmaster(NPC.NPC npc);
    }
}
