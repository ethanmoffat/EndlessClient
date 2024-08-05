using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol.Net;
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
        /// <param name="data">Packet data payload (any additional data that should be in the packet)</param>
        internal static void SetupReceivedPacketHasHeader<T>(this Mock<IPacketSendService> packetSendServiceMock, params byte[] data)
            where T : IPacket
        {
            IPacket receivedPacket = (IPacket)Activator.CreateInstance(typeof(T));
            receivedPacket.Deserialize(new EoReader(data));
            packetSendServiceMock.Setup(x => x.SendEncodedPacketAndWaitAsync(It.IsAny<IPacket>()))
                                 .Returns(Task.FromResult(receivedPacket));
        }
    }
}