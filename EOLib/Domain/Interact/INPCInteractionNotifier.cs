using AutomaticTypeMapper;
using EOLib.Domain.Interact.Citizen;
using EOLib.Domain.Interact.Skill;
using EOLib.IO;

namespace EOLib.Domain.Interact
{
    public interface INPCInteractionNotifier
    {
        void NotifyInteractionFromNPC(NPCType npcType);

        void NotifySkillLearnSuccess(int spellId, int characterGold);

        void NotifySkillLearnFail(SkillmasterReply reply, int classId);

        void NotifySkillForget();

        void NotifyStatReset();

        void NotifyCitizenUnsubscribe(CitizenUnsubscribeReply reply);

        void NotifyCitizenSignUp(int questionsWrong);

        void NotifyCitizenRequestSleep(int sleepCost);
    }

    [AutoMappedType]
    public class NoOpNPCInteractionNotifier : INPCInteractionNotifier
    {
        public void NotifyInteractionFromNPC(NPCType npcType) { }

        public void NotifySkillLearnSuccess(int spellId, int characterGold) { }

        public void NotifySkillLearnFail(SkillmasterReply reply, int classId) { }

        public void NotifySkillForget() { }

        public void NotifyStatReset() { }

        public void NotifyCitizenUnsubscribe(CitizenUnsubscribeReply reply) { }

        public void NotifyCitizenSignUp(int questionsWrong) { }

        public void NotifyCitizenRequestSleep(int sleepCost) { }
    }
}
