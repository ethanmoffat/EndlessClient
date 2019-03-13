// Original Work Copyright (c) Ethan Moffat 2014-2017
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
    public class AdminShowHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.AdminInteract;
        public override PacketAction Action => PacketAction.Agree;

        public AdminShowHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();

            if (id == _characterRepository.MainCharacter.ID)
                _characterRepository.MainCharacter = Shown(_characterRepository.MainCharacter);
            else
            {
                ICharacter character;
                try
                {
                    character = _currentMapStateRepository.Characters.Single(x => x.ID == id);
                }
                catch (InvalidOperationException) { return false; }

                var updatedCharacter = Shown(character);
                _currentMapStateRepository.Characters.Remove(character);
                _currentMapStateRepository.Characters.Add(updatedCharacter);
            }

            return true;
        }

        private static ICharacter Shown(ICharacter input)
        {
            var renderProps = input.RenderProperties.WithIsHidden(false);
            return input.WithRenderProperties(renderProps);
        }
    }
}
