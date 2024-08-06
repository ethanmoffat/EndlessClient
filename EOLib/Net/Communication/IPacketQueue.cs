using System.Collections.Generic;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Communication
{
    public interface IPacketQueue
    {
        int QueuedPacketCount { get; }

        void EnqueuePacketForHandling(IPacket pkt);

        IPacket PeekPacket();

        IPacket DequeueFirstPacket();

        IEnumerable<IPacket> DequeueAllPackets();
    }
}
