// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
    public interface IPacketSendService
    {
        void SendPacket(IPacket packet);

        Task SendPacketAsync(IPacket packet);

        Task<IPacket> SendRawPacketAndWaitAsync(IPacket packet);

        Task<IPacket> SendEncodedPacketAndWaitAsync(IPacket packet);
    }
}
