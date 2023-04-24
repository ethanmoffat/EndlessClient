using AutomaticTypeMapper;
using EOLib.Domain.Interact.Skill;
using EOLib.IO;

namespace EOLib.Domain.Interact
{
    public interface INPCInteractionNotifier
    {
        void NotifyInteractionFromNPC(NPCType npcType);

        void NotifySkillLearnSuccess(int spellId, int characterGold);

        void NotifySkillLearnFail(SkillmasterReply skillmasterReply, int classId);

        void NotifySkillForget();

        void NotifyStatReset();
    }

    [AutoMappedType]
    public class NoOpNPCInteractionNotifier : INPCInteractionNotifier
    {
        public void NotifyInteractionFromNPC(NPCType npcType) { }

        public void NotifySkillLearnSuccess(int spellId, int characterGold) { }

        public void NotifySkillLearnFail(SkillmasterReply skillmasterReply, int classId) { }

        public void NotifySkillForget() { }

        public void NotifyStatReset() { }
    }
}
