// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;

namespace EOLib.Net.Handlers
{
    public class PacketHandlerFinder : IPacketHandlerFinder
    {
        private readonly Dictionary<FamilyActionPair, IPacketHandler> _handlers;
 
        public PacketHandlerFinder(IPacketHandlerProvider packetHandlerProvider)
        {
            _handlers = packetHandlerProvider.PacketHandlers.ToDictionary(x => new FamilyActionPair(x.Family, x.Action));
        }

        public bool HandlerExists(PacketFamily family, PacketAction action)
        {
            var familyActionPair = new FamilyActionPair(family, action);
            return _handlers.ContainsKey(familyActionPair);
        }

        public IPacketHandler FindHandler(PacketFamily family, PacketAction action)
        {
            var familyActionPair = new FamilyActionPair(family, action);
            return _handlers[familyActionPair];
        }
    }
}