using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.Domain.Spells;
using System.Collections.Generic;

namespace EOLib.Domain.Notifiers
{
    public interface IOtherCharacterAnimationNotifier
    {
        void StartOtherCharacterWalkAnimation(int characterID, MapCoordinate destination, EODirection direction);

        void StartOtherCharacterAttackAnimation(int characterID, int noteIndex = -1);

        void NotifyStartSpellCast(int playerId, int spellId);

        void NotifyTargetNpcSpellCast(int playerId);

        void NotifySelfSpellCast(int playerId, int spellId, int spellHp, int percentHealth);

        void NotifyTargetOtherSpellCast(int sourcePlayerID, int targetPlayerID, int spellId, int recoveredHP, int targetPercentHealth);

        void NotifyGroupSpellCast(int playerId, int spellId, int spellHp, List<GroupSpellTarget> spellTargets);
    }

    [AutoMappedType]
    public class NoOpOtherCharacterAnimationNotifier : IOtherCharacterAnimationNotifier
    {
        public void StartOtherCharacterWalkAnimation(int characterID, MapCoordinate destination, EODirection direction) { }

        public void StartOtherCharacterAttackAnimation(int characterID, int noteIndex = -1) { }

        public void NotifyStartSpellCast(int playerId, int spellId) { }

        public void NotifyTargetNpcSpellCast(int playerId) { }

        public void NotifySelfSpellCast(int playerId, int spellId, int spellHp, int percentHealth) { }

        public void NotifyTargetOtherSpellCast(int sourcePlayerID, int targetPlayerID, int spellId, int recoveredHP, int targetPercentHealth) { }

        public void NotifyGroupSpellCast(int playerId, int spellId, int spellHp, List<GroupSpellTarget> spellTargets) { }
    }
}