// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.Communication;

namespace EOLib.Net.Handlers
{
    public class OutOfBandPacketHandler : IOutOfBandPacketHandler
    {
        private readonly IPacketQueueProvider _packetQueueProvider;
        private readonly IPacketHandlerFinderService _packetHandlerFinder;

        public OutOfBandPacketHandler(IPacketQueueProvider packetQueueProvider,
                                      IPacketHandlerFinderService packetHandlerFinder)
        {
            _packetQueueProvider = packetQueueProvider;
            _packetHandlerFinder = packetHandlerFinder;
        }

        public void PollForPacketsAndHandle()
        {
            if (OutOfBandPacketQueue.QueuedPacketCount == 0)
                return;

            if (Constants.OutOfBand_Packets_Handled_Per_Update >= OutOfBandPacketQueue.QueuedPacketCount)
            {
                var packets = OutOfBandPacketQueue.DequeueAllPackets();
                foreach (var nextPacket in packets)
                    FindAndHandlePacket(nextPacket);
            }
            else
            {
                for (int i = 0; i < Constants.OutOfBand_Packets_Handled_Per_Update && OutOfBandPacketQueue.QueuedPacketCount > 0; ++i)
                {
                    var nextPacket = OutOfBandPacketQueue.DequeueFirstPacket();
                    if (!FindAndHandlePacket(nextPacket))
                        i -= 1;
                }
            }
        }

        private bool FindAndHandlePacket(IPacket packet)
        {
            if (!_packetHandlerFinder.HandlerExists(packet.Family, packet.Action))
                return false;

            var handler = _packetHandlerFinder.FindHandler(packet.Family, packet.Action);
            if (!handler.CanHandle)
                return false;

            handler.HandlePacket(packet);
            return true;
        }

        private IPacketQueue OutOfBandPacketQueue { get { return _packetQueueProvider.HandleOutOfBandPacketQueue; } }
    }
}
