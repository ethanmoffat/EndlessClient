// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;

namespace EOLib.Net.Handlers
{
    public class PacketHandlingTypeFinder : IPacketHandlingTypeFinder
    {
        private readonly List<FamilyActionPair> _inBandPackets;
        private readonly List<FamilyActionPair> _outOfBandPackets;

        public PacketHandlingTypeFinder(IEnumerable<IPacketHandler> outOfBandHandlers)
        {
            _inBandPackets = new List<FamilyActionPair>
            {
                new FamilyActionPair(PacketFamily.Init, PacketAction.Init),
                new FamilyActionPair(PacketFamily.Account, PacketAction.Reply),
                new FamilyActionPair(PacketFamily.Character, PacketAction.Player),
                new FamilyActionPair(PacketFamily.Character, PacketAction.Reply),
                new FamilyActionPair(PacketFamily.Login, PacketAction.Reply),
                new FamilyActionPair(PacketFamily.Welcome, PacketAction.Reply)
            };

            _outOfBandPackets = outOfBandHandlers.Select(x => new FamilyActionPair(x.Family, x.Action)).ToList();
        }

        public PacketHandlingType FindHandlingType(PacketFamily family, PacketAction action)
        {
            var fap = new FamilyActionPair(family, action);

            if (_inBandPackets.Contains(fap))
                return PacketHandlingType.InBand;

            if (_outOfBandPackets.Contains(fap))
                return PacketHandlingType.OutOfBand;

            return PacketHandlingType.NotHandled;
        }
    }
}