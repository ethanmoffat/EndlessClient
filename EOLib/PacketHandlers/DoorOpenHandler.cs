using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class DoorOpenHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var x = packet.ReadChar();
            var y = packet.ReadShort();


            if (_currentMapStateRepository.OpenDoors.Any(d => d.X == x && d.Y == y))
                return true;

            Warp warp;
            try
            {
                warp = _currentMapStateRepository.PendingDoors.Single(w => w.X == x && w.Y == y);
            }
            catch
            {
                // another player attempted to open a door
                warp = new Warp(_currentMapProvider.CurrentMap.Warps[y, x]);
            }

            _currentMapStateRepository.PendingDoors.Remove(warp);
            _currentMapStateRepository.OpenDoors.Add(warp);

            return true;
        }
    }
}
