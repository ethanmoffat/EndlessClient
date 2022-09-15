using AutomaticTypeMapper;
using EOLib.Domain.Spells;
using System.Collections.Generic;

namespace EOLib.Domain.Notifiers
{
    public interface IOtherCharacterAnimationNotifier
    {
        void StartOtherCharacterWalkAnimation(int characterID, byte destinationX, byte destinationY, EODirection direction);

        void StartOtherCharacterAttackAnimation(int characterID, int noteIndex = -1);

        void NotifyStartSpellCast(short playerId, short spellId);

        void NotifyTargetNpcSpellCast(short playerId);

        void NotifySelfSpellCast(short playerId, short spellId, int spellHp, byte percentHealth);

        void NotifyTargetOtherSpellCast(short sourcePlayerID, short targetPlayerID, short spellId, int recoveredHP, byte targetPercentHealth);

        void NotifyGroupSpellCast(short playerId, short spellId, short spellHp, List<GroupSpellTarget> spellTargets);
    }

    [AutoMappedType]
    public class NoOpOtherCharacterAnimationNotifier : IOtherCharacterAnimationNotifier
    {
        public void StartOtherCharacterWalkAnimation(int characterID, byte destinationX, byte destinationY, EODirection direction) { }

        public void StartOtherCharacterAttackAnimation(int characterID, int noteIndex = -1) { }

        public void NotifyStartSpellCast(short playerId, short spellId) { }

        public void NotifyTargetNpcSpellCast(short playerId) { }

        public void NotifySelfSpellCast(short playerId, short spellId, int spellHp, byte percentHealth) { }

        public void NotifyTargetOtherSpellCast(short sourcePlayerID, short targetPlayerID, short spellId, int recoveredHP, byte targetPercentHealth) { }

        public void NotifyGroupSpellCast(short playerId, short spellId, short spellHp, List<GroupSpellTarget> spellTargets) { }
    }
}
