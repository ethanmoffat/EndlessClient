using AutomaticTypeMapper;

namespace EOLib.Net.Connection
{
    public interface IConnectionStateRepository
    {
        bool NeedsReconnect { get; set; }
    }

    public interface IConnectionStateProvider
    {
        bool NeedsReconnect { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ConnectionStateRepository : IConnectionStateRepository, IConnectionStateProvider
    {
        public bool NeedsReconnect { get; set; }
    }
}
