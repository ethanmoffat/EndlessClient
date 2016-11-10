// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.Net.Handlers
{
    public class PacketHandlerProvider : IPacketHandlerProvider
    {
        public PacketHandlerProvider(IEnumerable<IPacketHandler> packetHandlers)
        {
            PacketHandlers = packetHandlers;
        }

        public IEnumerable<IPacketHandler> PacketHandlers { get; private set; }
    }

    public interface IPacketHandlerProvider
    {
        IEnumerable<IPacketHandler> PacketHandlers { get; }
    }
}
