// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    public class PlayerLeaveMapHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Avatar;

        public override PacketAction Action => PacketAction.Remove;

        public PlayerLeaveMapHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();

            //todo: need to signal client that animation should be performed
            var anim = WarpAnimation.None;
            if (packet.ReadPosition < packet.Length)
                anim = (WarpAnimation)packet.ReadChar();

            ICharacter character;
            try
            {
                character = _currentMapStateRepository.Characters.Single(x => x.ID == id);
            }
            catch (InvalidOperationException) { return false; }

            _currentMapStateRepository.Characters.Remove(character);

            return true;
        }
    }
}
