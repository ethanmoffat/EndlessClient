using System;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
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
            _currentMapStateRepository.VisibleSpikeTraps.Remove(new MapCoordinate(character.RenderProperties.MapX, character.RenderProperties.MapY));

            return true;
        }
    }
}
