// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Notifiers
{
    //this interface could get very busy. it may need to be split into multiple interfaces
    public interface IMainCharacterEventNotifier
    {
        void NotifyGainedExp(int expDifference);

        void NotifyDead();

        void NotifyTakeDamage(int damageTaken, int playerPercentHealth);
    }

    public class NoOpMainCharacterEventNotifier : IMainCharacterEventNotifier
    {
        public void NotifyGainedExp(int expDifference) { }

        public void NotifyDead() { }

        public void NotifyTakeDamage(int damageTaken, int playerPercentHealth) { }
    }
}
