using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface INPCActionNotifier
    {
        void StartNPCWalkAnimation(int npcIndex);

        void StartNPCAttackAnimation(int npcIndex);

        void RemoveNPCFromView(int npcIndex, int playerId, Optional<short> spellId, Optional<int> damage, bool showDeathAnimation);

        void ShowNPCSpeechBubble(int npcIndex, string message);

        void NPCTakeDamage(short npcIndex, int fromPlayerId, int damageToNpc, short npcPctHealth, Optional<int> spellId);
    }

    [AutoMappedType]
    public class NoOpNPCActionNotifier : INPCActionNotifier
    {
        public void StartNPCWalkAnimation(int npcIndex) { }

        public void StartNPCAttackAnimation(int npcIndex) { }

        public void RemoveNPCFromView(int npcIndex, int playerId, Optional<short> spellId, Optional<int> damage, bool showDeathAnimation) { }

        public void ShowNPCSpeechBubble(int npcIndex, string message) { }

        public void NPCTakeDamage(short npcIndex, int fromPlayerId, int damageToNpc, short npcPctHealth, Optional<int> spellId) { }
    }
}
