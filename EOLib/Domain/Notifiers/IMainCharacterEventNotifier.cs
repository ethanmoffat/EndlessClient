using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    //this interface could get very busy. it may need to be split into multiple interfaces
    public interface IMainCharacterEventNotifier
    {
        void NotifyGainedExp(int expDifference);

        void NotifyTakeDamage(int damageTaken, int playerPercentHealth);

        void TakeItemFromMap(short id, int amountTaken);
    }

    [AutoMappedType]
    public class NoOpMainCharacterEventNotifier : IMainCharacterEventNotifier
    {
        public void NotifyGainedExp(int expDifference) { }

        public void NotifyTakeDamage(int damageTaken, int playerPercentHealth) { }

        public void TakeItemFromMap(short id, int amountTaken) { }
    }
}
