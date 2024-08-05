using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using DomainWarp = EOLib.Domain.Map.Warp;

namespace EOLib.PacketHandlers.Door
{
    /// <summary>
    /// Sent when a player near you opens a door
    /// </summary>
    [AutoMappedType]
    public class DoorOpenHandler : InGameOnlyPacketHandler<DoorOpenServerPacket>
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICurrentMapProvider _currentMapProvider;

        public override PacketFamily Family => PacketFamily.Door;

        public override PacketAction Action => PacketAction.Open;

        public DoorOpenHandler(IPlayerInfoProvider playerInfoProvider,
                               ICurrentMapStateRepository currentMapStateRepository,
                               ICurrentMapProvider currentMapProvider)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _currentMapProvider = currentMapProvider;
        }

        public override bool HandlePacket(DoorOpenServerPacket packet)
        {
            var x = packet.Coords.X;
            var y = packet.Coords.Y;

            if (_currentMapStateRepository.OpenDoors.Any(d => d.X == x && d.Y == y))
                return true;

            DomainWarp warp;
            try
            {
                warp = _currentMapStateRepository.PendingDoors.Single(w => w.X == x && w.Y == y);
            }
            catch
            {
                // another player attempted to open a door
                warp = new DomainWarp(_currentMapProvider.CurrentMap.Warps[y, x]);
            }

            _currentMapStateRepository.PendingDoors.Remove(warp);
            _currentMapStateRepository.OpenDoors.Add(warp);

            return true;
        }
    }
}