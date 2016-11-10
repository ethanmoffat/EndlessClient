// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    public class MainPlayerWalkHandler : IPacketHandler
    {
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public PacketFamily Family { get { return PacketFamily.Walk; } }
        public PacketAction Action { get { return PacketAction.Reply; } }

        public bool CanHandle { get { return _playerInfoProvider.PlayerIsInGame; } }

        public MainPlayerWalkHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICurrentMapStateRepository currentMapStateRepository)
        {
            _playerInfoProvider = playerInfoProvider;
            _currentMapStateRepository = currentMapStateRepository;
        }

        public bool HandlePacket(IPacket packet)
        {
            if (packet.ReadByte() != 255 || packet.ReadByte() != 255)
                return false;

            var numberOfMapItems = packet.PeekEndString().Length / 9;
            for (int i = 0; i < numberOfMapItems; ++i)
            {
                var uid = packet.ReadShort();
                var itemID = packet.ReadShort();
                var x = packet.ReadChar();
                var y = packet.ReadChar();
                var amount = packet.ReadThree();

                var newItem = new MapItem(uid, itemID, x, y).WithAmount(amount);
                _currentMapStateRepository.MapItems.Add(newItem);
            }

            return true;
        }

        public async Task<bool> HandlePacketAsync(IPacket packet)
        {
            return await Task.Run(() => HandlePacket(packet));
        }
    }
}
