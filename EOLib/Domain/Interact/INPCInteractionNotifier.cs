using AutomaticTypeMapper;
using EOLib.Domain.Interact.Skill;
using EOLib.IO;

namespace EOLib.Domain.Interact
{
    public interface INPCInteractionNotifier
    {
        void NotifyInteractionFromNPC(NPCType npcType);

        void NotifySkillLearnFail(SkillmasterReply skillmasterReply, short classId);
    }

    [AutoMappedType]
    public class NoOpNPCInteractionNotifier : INPCInteractionNotifier
    {
        public void NotifyInteractionFromNPC(NPCType npcType) { }

        public void NotifySkillLearnFail(SkillmasterReply skillmasterReply, short classId) { }
    }
}
