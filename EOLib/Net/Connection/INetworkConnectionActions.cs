using System.Threading.Tasks;
using EOLib.Domain.Protocol;
using EOLib.Net.Communication;

namespace EOLib.Net.Connection
{
    public interface INetworkConnectionActions
    {
        Task<ConnectResult> ConnectToServer();

        void DisconnectFromServer();

        Task<IInitializationData> BeginHandshake();

        void CompleteHandshake(IInitializationData initializationData);
    }
}
