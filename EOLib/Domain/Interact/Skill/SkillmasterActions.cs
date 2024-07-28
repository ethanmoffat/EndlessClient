using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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
            var packet = new StatSkillTakeClientPacket
            {
                SessionId = _skillDataProvider.ID,
                SpellId = spellId
            };
            _packetSendService.SendPacket(packet);
        }

        public void ForgetSkill(int spellId)
        {
            var packet = new StatSkillRemoveClientPacket
            {
                SessionId = _skillDataProvider.ID,
                SpellId = spellId
            };
            _packetSendService.SendPacket(packet);
        }

        public void ResetCharacter()
        {
            var packet = new StatSkillJunkClientPacket
            {
                SessionId = _skillDataProvider.ID,
            };
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