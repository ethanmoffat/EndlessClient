using AutomaticTypeMapper;
using EOLib.Domain.Interact.Quest;
using EOLib.IO.Repositories;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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
            var packet = new ShopOpenClientPacket { NpcIndex = npc.Index };
            _packetSendService.SendPacket(packet);
        }

        public void RequestQuest(NPC.NPC npc)
        {
            _questDataRepository.RequestedNPC = npc;

            var packet = new QuestUseClientPacket
            {
                NpcIndex = npc.Index,
                QuestId = _enfFileProvider.ENFFile[npc.ID].VendorID
            };
            _packetSendService.SendPacket(packet);
        }

        public void RequestBank(NPC.NPC npc)
        {
            var packet = new BankOpenClientPacket { NpcIndex = npc.Index };
            _packetSendService.SendPacket(packet);
        }

        public void RequestSkillmaster(NPC.NPC npc)
        {
            var packet = new StatSkillOpenClientPacket { NpcIndex = npc.Index };
            _packetSendService.SendPacket(packet);
        }

        public void RequestInnkeeper(NPC.NPC npc)
        {
            var packet = new CitizenOpenClientPacket { NpcIndex = npc.Index };
            _packetSendService.SendPacket(packet);
        }

        public void RequestLaw(NPC.NPC npc)
        {
            var packet = new MarriageOpenClientPacket { NpcIndex = npc.Index };
            _packetSendService.SendPacket(packet);
        }

        public void RequestPriest(NPC.NPC npc)
        {
            var packet = new PriestOpenClientPacket { NpcIndex = npc.Index };
            _packetSendService.SendPacket(packet);
        }

        public void RequestBarber(NPC.NPC npc)
        {
            var packet = new BarberOpenClientPacket { NpcIndex = npc.Index };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IMapNPCActions
    {
        void RequestShop(NPC.NPC npc);
        void RequestQuest(NPC.NPC npc);
        void RequestBank(NPC.NPC npc);
        void RequestSkillmaster(NPC.NPC npc);
        void RequestInnkeeper(NPC.NPC npc);
        void RequestLaw(NPC.NPC npc);
        void RequestPriest(NPC.NPC npc);
        void RequestBarber(NPC.NPC npc);
    }

}