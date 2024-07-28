using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Threading.Tasks;

namespace EOLib.Net.Connection
{
    public interface INetworkConnectionActions
    {
        Task<ConnectResult> ConnectToServer();

        void DisconnectFromServer();

        Task<InitInitServerPacket> BeginHandshake(int challenge);

        void CompleteHandshake(InitInitServerPacket initializationData);
    }
}