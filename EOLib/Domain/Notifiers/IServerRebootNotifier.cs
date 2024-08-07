using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface IServerRebootNotifier
    {
        void NotifyServerReboot();
    }

    [AutoMappedType]
    public class NoOpServerRebootNotifier : IServerRebootNotifier
    {
        public void NotifyServerReboot() { }
    }
}
