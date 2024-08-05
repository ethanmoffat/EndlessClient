using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Handlers
{
    [AutoMappedType]
    public class PacketHandlerFinder : IPacketHandlerFinder
    {
        private readonly IReadOnlyDictionary<FamilyActionPair, IPacketHandler> _handlers;

        public PacketHandlerFinder(IPacketHandlerProvider packetHandlerProvider)
        {
            _handlers = packetHandlerProvider.PacketHandlers
                                             .ToDictionary(x => new FamilyActionPair(x.Family, x.Action));
        }

        public bool HandlerExists(PacketFamily family, PacketAction action)
        {
            return _handlers.ContainsKey(new FamilyActionPair(family, action));
        }

        public IPacketHandler FindHandler(PacketFamily family, PacketAction action)
        {
            return _handlers[new FamilyActionPair(family, action)];
        }
    }

    public interface IPacketHandlerFinder
    {
        bool HandlerExists(PacketFamily family, PacketAction action);

        IPacketHandler FindHandler(PacketFamily family, PacketAction action);
    }
}
