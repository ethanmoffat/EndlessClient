namespace EOLib.Net.Communication
{
    public interface INetworkClientFactory
    {
        INetworkClient CreateNetworkClient(int timeout = Constants.ResponseTimeout);
    }
}
