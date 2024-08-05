using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Handlers
{
    [AutoMappedType]
    public class PacketHandlingTypeFinder : IPacketHandlingTypeFinder
    {
        private readonly List<FamilyActionPair> _inBandPackets;
        private readonly List<FamilyActionPair> _outOfBandPackets;

        public PacketHandlingTypeFinder(IEnumerable<IPacketHandler> outOfBandHandlers)
        {
            //Packets that are handled in-band are manually defined here.
            //All other Family/Action pairs should be handled by the OutOfBandPacketHandler
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

    public interface IPacketHandlingTypeFinder
    {
        PacketHandlingType FindHandlingType(PacketFamily family, PacketAction action);
    }
}
