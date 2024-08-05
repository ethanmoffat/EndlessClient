using EOLib.IO.Map;

namespace EOLib.Domain.Notifiers
{
    public interface IChestEventNotifier
    {
        void NotifyChestLocked(ChestKey key);

        void NotifyChestBroken();
    }

    public class NoOpChestEventNotifier : IChestEventNotifier
    {
        public void NotifyChestLocked(ChestKey key) { }

        public void NotifyChestBroken() { }
    }
}