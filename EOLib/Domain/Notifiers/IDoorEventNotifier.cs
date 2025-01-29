using AutomaticTypeMapper;
using EOLib.IO.Map;

namespace EOLib.Domain.Notifiers
{
    public interface IDoorEventNotifier
    {
        void NotifyDoorLocked(ChestKey key);
    }

    [AutoMappedType]
    public class NoOpDoorEventNotifier : IDoorEventNotifier
    {
        public void NotifyDoorLocked(ChestKey key) { }
    }
}
