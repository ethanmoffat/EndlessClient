using EOLib.IO.Map;

namespace EndlessClient.Subscribers
{
    public interface IChestEventSubscriber
    {
        void NotifyChestBroken();

        void NotifyChestLocked(ChestKey key);
    }
}
