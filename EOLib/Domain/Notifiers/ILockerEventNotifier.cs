using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface ILockerEventNotifier
    {
        void NotifyLockerFull(int maxItems);
    }

    [AutoMappedType]
    public class NoOpLockerEventNotifier : ILockerEventNotifier
    {
        public void NotifyLockerFull(int maxItems) { }
    }
}
