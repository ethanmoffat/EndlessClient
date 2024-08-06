using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    //this interface could get very busy. it may need to be split into multiple interfaces
    public interface IMainCharacterEventNotifier
    {
        void NotifyGainedExp(int expDifference);

        void NotifyTakeDamage(int damageTaken, int playerPercentHealth, bool isHeal);

        void TakeItemFromMap(int id, int amountTaken);

        void DropItem(int id, int amountDropped);

        void JunkItem(int id, int amountRemoved);
    }

    [AutoMappedType]
    public class NoOpMainCharacterEventNotifier : IMainCharacterEventNotifier
    {
        public void NotifyGainedExp(int expDifference) { }

        public void NotifyTakeDamage(int damageTaken, int playerPercentHealth, bool isHeal) { }

        public void TakeItemFromMap(int id, int amountTaken) { }

        public void DropItem(int id, int amountDropped) { }

        public void JunkItem(int id, int amountTaken) { }
    }
}
