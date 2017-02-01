// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Notifiers
{
    public interface IOtherCharacterEventNotifier
    {
        void OtherCharacterTakeDamage(int characterID, int playerPercentHealth, int damageTaken);
    }

    public class NoOpOtherCharacterEventNotifier : IOtherCharacterEventNotifier
    {
        public void OtherCharacterTakeDamage(int characterID, int playerPercentHealth, int damageTaken) { }
    }
}
