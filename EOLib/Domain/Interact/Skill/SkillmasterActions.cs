using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact.Skill
{
    [AutoMappedType]
    public class SkillmasterActions : ISkillmasterActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly ISkillDataProvider _skillDataProvider;

        public SkillmasterActions(IPacketSendService packetSendService,
                                  ISkillDataProvider skillDataProvider)
        {
            _packetSendService = packetSendService;
            _skillDataProvider = skillDataProvider;
        }

        public void LearnSkill(int spellId)
        {
            var packet = new PacketBuilder(PacketFamily.StatSkill, PacketAction.Take)
                .AddInt(_skillDataProvider.ID)
                .AddShort(spellId)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void ForgetSkill(int spellId)
        {
            var packet = new PacketBuilder(PacketFamily.StatSkill, PacketAction.Remove)
                .AddInt(_skillDataProvider.ID)
                .AddShort(spellId)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void ResetCharacter()
        {
            var packet = new PacketBuilder(PacketFamily.StatSkill, PacketAction.Junk)
                .AddInt(_skillDataProvider.ID)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface ISkillmasterActions
    {
        void LearnSkill(int spellId);

        void ForgetSkill(int spellId);

        void ResetCharacter();
    }
}
