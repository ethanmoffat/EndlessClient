using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
    public interface ISafeNetworkOperation
    {
        Task<bool> Invoke();
    }

    public interface ISafeNetworkOperation<out T> : ISafeNetworkOperation
    {
        T Result { get; }
    }
}
