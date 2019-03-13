// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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

        public override PacketFamily Family => PacketFamily.Door;

        public override PacketAction Action => PacketAction.Open;

        public DoorOpenHandler(IPlayerInfoProvider playerInfoProvider,
                               ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var x = packet.ReadChar();
            var y = packet.ReadShort();

            IWarp warp;
            try
            {
                warp = _currentMapStateRepository.PendingDoors.Single(w => w.X == x && w.Y == y);
            }
            catch { return false; }

            _currentMapStateRepository.PendingDoors.Remove(warp);
            _currentMapStateRepository.OpenDoors.Add(warp);

            return true;
        }
    }
}
