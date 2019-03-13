// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
    public class PlayerDirectionHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _mapStateRepository;

        public override PacketFamily Family => PacketFamily.Face;

        public override PacketAction Action => PacketAction.Player;

        public PlayerDirectionHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICurrentMapStateRepository mapStateRepository)
            : base(playerInfoProvider)
        {
            _mapStateRepository = mapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();
            var direction = (EODirection) packet.ReadChar();

            ICharacter character;
            try
            {
                character = _mapStateRepository.Characters.Single(x => x.ID == id);
            }
            catch (InvalidOperationException) { return false; } //more than 1 character with that ID - thrown by .Single()

            var newRenderProps = character.RenderProperties.WithDirection(direction);
            var newCharacter = character.WithRenderProperties(newRenderProps);

            _mapStateRepository.Characters.Remove(character);
            _mapStateRepository.Characters.Add(newCharacter);

            return true;
        }
    }
}
