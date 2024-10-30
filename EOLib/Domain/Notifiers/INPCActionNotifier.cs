using AutomaticTypeMapper;
using EOLib.Domain.Map;
using Optional;

namespace EOLib.Domain.Notifiers
{
    public interface INPCActionNotifier
    {
        void StartNPCWalkAnimation(int npcIndex, MapCoordinate coords, EODirection direction);

        void StartNPCAttackAnimation(int npcIndex, EODirection direction);

        void RemoveNPCFromView(int npcIndex, int playerId, Option<int> spellId, Option<int> damage, bool showDeathAnimation);

        void ShowNPCSpeechBubble(int npcIndex, string message);

        void NPCTakeDamage(int npcIndex, int fromPlayerId, int damageToNpc, int npcPctHealth, Option<int> spellId);

        void NPCDropItem(MapItem item);
    }

    [AutoMappedType]
    public class NoOpNPCActionNotifier : INPCActionNotifier
    {
        public void StartNPCWalkAnimation(int npcIndex, MapCoordinate coords, EODirection direction) { }

        public void StartNPCAttackAnimation(int npcIndex, EODirection direction) { }

        public void RemoveNPCFromView(int npcIndex, int playerId, Option<int> spellId, Option<int> damage, bool showDeathAnimation) { }

        public void ShowNPCSpeechBubble(int npcIndex, string message) { }

        public void NPCTakeDamage(int npcIndex, int fromPlayerId, int damageToNpc, int npcPctHealth, Option<int> spellId) { }

        public void NPCDropItem(MapItem item) { }
    }
}
