using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Net.Handlers
{
    [AutoMappedType(IsSingleton = true)]
    public class PacketHandlerProvider : IPacketHandlerProvider
    {
        public PacketHandlerProvider(IEnumerable<IPacketHandler> packetHandlers)
        {
            PacketHandlers = packetHandlers;
        }

        public IEnumerable<IPacketHandler> PacketHandlers { get; }
    }

    public interface IPacketHandlerProvider
    {
        IEnumerable<IPacketHandler> PacketHandlers { get; }
    }
}