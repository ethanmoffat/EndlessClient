﻿using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using EOLib.Net;
using EOLib.Net.Communication;
using Moq;

namespace EOLib.Test.TestHelpers
{
    [ExcludeFromCodeCoverage]
    internal static class PacketSendServiceHelpers
    {
        /// <summary>
        /// Setup the PacketSendService mock to return a packet with the specified family/action/data from SendEncodedPacketAndWaitAsync
        /// </summary>
        /// <param name="packetSendServiceMock">The mocked packet send service</param>
        /// <param name="family">Packet family for the "received" packet</param>
        /// <param name="action">Packet action for the "received" packet</param>
        /// <param name="data">Packet data payload (any additional data that should be in the packet)</param>
        internal static void SetupReceivedPacketHasHeader(this Mock<IPacketSendService> packetSendServiceMock,
                                                          PacketFamily family,
                                                          PacketAction action,
                                                          params byte[] data)
        {
            var receivedPacket = new PacketBuilder(family, action)
                .AddBytes(data)
                .Build();

            packetSendServiceMock.Setup(x => x.SendEncodedPacketAndWaitAsync(It.IsAny<IPacket>()))
                                 .Returns(Task.FromResult(receivedPacket));
        }
    }
}
