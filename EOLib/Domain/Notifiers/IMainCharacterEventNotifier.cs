using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    //this interface could get very busy. it may need to be split into multiple interfaces
    public interface IMainCharacterEventNotifier
    {
        void NotifyGainedExp(int expDifference);

        void NotifyTakeDamage(int damageTaken, int playerPercentHealth, bool isHeal);

        void TakeItemFromMap(short id, int amountTaken);

        void DropItem(short id, int amountDropped);

        void JunkItem(short id, int amountRemoved);
    }

    [AutoMappedType]
    public class NoOpMainCharacterEventNotifier : IMainCharacterEventNotifier
    {
        public void NotifyGainedExp(int expDifference) { }

        public void NotifyTakeDamage(int damageTaken, int playerPercentHealth, bool isHeal) { }

        public void TakeItemFromMap(short id, int amountTaken) { }

        public void DropItem(short id, int amountDropped) { }

        public void JunkItem(short id, int amountTaken) { }
    }
}
