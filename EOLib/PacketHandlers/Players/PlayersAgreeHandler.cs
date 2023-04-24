﻿using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Extensions;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Players
{
    /// <summary>
    /// Sent when a player is entering the map
    /// </summary>
    [AutoMappedType]
    public class PlayersAgreeHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _mapStateRepository;
        private readonly ICharacterFromPacketFactory _characterFromPacketFactory;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Players;

        public override PacketAction Action => PacketAction.Agree;

        public PlayersAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICharacterRepository characterRepository,
                                     ICurrentMapStateRepository mapStateRepository,
                                     ICharacterFromPacketFactory characterFromPacketFactory,
                                     IEIFFileProvider eifFileProvider,
                                     IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _mapStateRepository = mapStateRepository;
            _characterFromPacketFactory = characterFromPacketFactory;
            _eifFileProvider = eifFileProvider;
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Missing 255 header byte for player enter map handler", packet);

            var character = _characterFromPacketFactory.CreateCharacter(packet);

            if (packet.PeekByte() != 255) // next byte was the warp animation: sent on Map::Enter in eoserv
            {
                var anim = (WarpAnimation)packet.ReadChar();

                foreach (var notifier in _effectNotifiers)
                    notifier.NotifyWarpEnterEffect(character.ID, anim);
            }

            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Missing 255 byte after the warp animation for player enter map handler", packet);

            // 0 for NPC, 1 for player. In eoserv it is never 0.
            if (packet.ReadChar() != 1)
                throw new MalformedPacketException("Missing '1' char after warp animation for player enter map handler. Are you using a non-standard version of EOSERV?", packet);

            if (_characterRepository.MainCharacter.ID == character.ID)
            {
                var existingCharacter = _characterRepository.MainCharacter;
                var isRangedWeapon = _eifFileProvider.EIFFile.IsRangedWeapon(character.RenderProperties.WeaponGraphic);
                _characterRepository.MainCharacter = existingCharacter.WithAppliedData(character, isRangedWeapon);
                _characterRepository.HasAvatar = true;
            }
            else if (_mapStateRepository.Characters.ContainsKey(character.ID))
            {
                var existingCharacter = _mapStateRepository.Characters[character.ID];
                var isRangedWeapon = _eifFileProvider.EIFFile.IsRangedWeapon(character.RenderProperties.WeaponGraphic);
                _mapStateRepository.Characters[character.ID] = existingCharacter.WithAppliedData(character, isRangedWeapon);
            }
            else
            {
                _mapStateRepository.Characters[character.ID] = character;
            }

            return true;
        }
    }
}
