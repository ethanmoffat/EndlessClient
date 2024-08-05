using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface IOtherCharacterEventNotifier
    {
        void OtherCharacterTakeDamage(int characterID, int playerPercentHealth, int damageTaken, bool isHeal);

        void OtherCharacterSaySomething(int characterID, string message);

        void OtherCharacterSaySomethingToGroup(int characterID, string message);

        void AdminAnnounce(string message);
    }

    [AutoMappedType]
    public class NoOpOtherCharacterEventNotifier : IOtherCharacterEventNotifier
    {
        public void OtherCharacterTakeDamage(int characterID, int playerPercentHealth, int damageTaken, bool isHeal) { }

        public void OtherCharacterSaySomething(int characterID, string message) { }

        public void OtherCharacterSaySomethingToGroup(int characterID, string message) { }

        public void AdminAnnounce(string message) { }
    }
}