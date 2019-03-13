// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class MainPlayerWalkHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Walk;
        public override PacketAction Action => PacketAction.Reply;

        public MainPlayerWalkHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
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

                var newItem = new Item(uid, itemID, x, y).WithAmount(amount);
                _currentMapStateRepository.MapItems.Add(newItem);
            }

            return true;
        }
    }
}
