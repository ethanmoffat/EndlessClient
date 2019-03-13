// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class PlayerAttackHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _otherCharacterAnimationNotifiers;

        public override PacketFamily Family => PacketFamily.Attack;

        public override PacketAction Action => PacketAction.Player;

        public PlayerAttackHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _otherCharacterAnimationNotifiers = otherCharacterAnimationNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerID = packet.ReadShort();
            var direction = (EODirection)packet.ReadChar();

            ICharacter character;
            try
            {
                character = _currentMapStateRepository.Characters.Single(x => x.ID == playerID);
            }
            catch (InvalidOperationException) { return false; }

            var renderProperties = character.RenderProperties.WithDirection(direction);
            var newCharacter = character.WithRenderProperties(renderProperties);

            _currentMapStateRepository.Characters.Remove(character);
            _currentMapStateRepository.Characters.Add(newCharacter);

            foreach (var notifier in _otherCharacterAnimationNotifiers)
                notifier.StartOtherCharacterAttackAnimation(playerID);

            return true;
        }
    }
}
