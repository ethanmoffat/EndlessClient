using Moffat.EndlessOnline.SDK.Protocol.Net;
using System.Collections.Generic;

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
