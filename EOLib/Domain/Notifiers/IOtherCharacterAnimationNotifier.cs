using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface IOtherCharacterAnimationNotifier
    {
        void StartOtherCharacterWalkAnimation(int characterID);

        void StartOtherCharacterAttackAnimation(int characterID);
    }

    [AutoMappedType]
    public class NoOpOtherCharacterAnimationNotifier : IOtherCharacterAnimationNotifier
    {
        public void StartOtherCharacterWalkAnimation(int characterID) { }

        public void StartOtherCharacterAttackAnimation(int characterID) { }
    }
}
