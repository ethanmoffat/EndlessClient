using AutomaticTypeMapper;
using EOLib.Domain.Interact.Skill;
using EOLib.IO;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Interact
{
    public interface INPCInteractionNotifier
    {
        void NotifyInteractionFromNPC(NPCType npcType);

        void NotifySkillLearnSuccess(int spellId, int characterGold);

        void NotifySkillLearnFail(SkillmasterReply reply, int classId);

        void NotifySkillForget();

        void NotifyStatReset();

        void NotifyCitizenUnsubscribe(InnUnsubscribeReply reply);

        void NotifyCitizenSignUp(int questionsWrong);

        void NotifyCitizenRequestSleep(int sleepCost);

        void NotifyPriestReply(PriestReply reply);

        void NotifyPriestRequest(string partnerName);

        void NotifyMarriageReply(MarriageReply reply);
    }

    [AutoMappedType]
    public class NoOpNPCInteractionNotifier : INPCInteractionNotifier
    {
        public void NotifyInteractionFromNPC(NPCType npcType) { }

        public void NotifySkillLearnSuccess(int spellId, int characterGold) { }

        public void NotifySkillLearnFail(SkillmasterReply reply, int classId) { }

        public void NotifySkillForget() { }

        public void NotifyStatReset() { }

        public void NotifyCitizenUnsubscribe(InnUnsubscribeReply reply) { }

        public void NotifyCitizenSignUp(int questionsWrong) { }

        public void NotifyCitizenRequestSleep(int sleepCost) { }

        public void NotifyPriestReply(PriestReply reply) { }

        public void NotifyPriestRequest(string partnerName) { }

        public void NotifyMarriageReply(MarriageReply reply) { }
    }
}
