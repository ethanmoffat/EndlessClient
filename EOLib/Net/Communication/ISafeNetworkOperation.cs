// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
