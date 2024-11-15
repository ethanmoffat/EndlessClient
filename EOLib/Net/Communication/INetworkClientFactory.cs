namespace EOLib.Net.Communication
{
    public interface INetworkClientFactory
    {
        INetworkClient CreateNetworkClient(int timeout = TimeoutConstants.ResponseTimeout);
    }
}
