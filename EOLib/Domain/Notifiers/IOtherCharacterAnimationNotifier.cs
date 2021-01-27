using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface IOtherCharacterAnimationNotifier
    {
        void StartOtherCharacterWalkAnimation(int characterID);

        void StartOtherCharacterAttackAnimation(int characterID);

        void NotifyStartSpellCast(short playerId, short spellId);

        void NotifySelfSpellCast(short playerId, short spellId, int spellHp, byte percentHealth);

        void NotifyTargetOtherSpellCast(short sourcePlayerID, short targetPlayerID, short spellId, int recoveredHP, byte targetPercentHealth);
    }

    [AutoMappedType]
    public class NoOpOtherCharacterAnimationNotifier : IOtherCharacterAnimationNotifier
    {
        public void StartOtherCharacterWalkAnimation(int characterID) { }

        public void StartOtherCharacterAttackAnimation(int characterID) { }

        public void NotifyStartSpellCast(short playerId, short spellId) { }

        public void NotifySelfSpellCast(short playerId, short spellId, int spellHp, byte percentHealth) { }

        public void NotifyTargetOtherSpellCast(short sourcePlayerID, short targetPlayerID, short spellId, int recoveredHP, byte targetPercentHealth) { }
    }
}
