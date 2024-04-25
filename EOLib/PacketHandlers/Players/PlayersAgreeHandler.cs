using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
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
        private readonly IPacketTranslator<PlayersAgreeData> _playersAgreeTranslator;
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
                                     IEnumerable<IEffectNotifier> effectNotifiers,
                                     IPacketTranslator<PlayersAgreeData> playersAgreeTranslator)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _mapStateRepository = mapStateRepository;
            _characterFromPacketFactory = characterFromPacketFactory;
            _eifFileProvider = eifFileProvider;
            _effectNotifiers = effectNotifiers;
            _playersAgreeTranslator = playersAgreeTranslator;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var data = _playersAgreeTranslator.TranslatePacket(packet);
            var character = data.Characters[0];

            if (_characterRepository.MainCharacter.ID == character.ID)
            {
                var existingCharacter = _characterRepository.MainCharacter;
                _characterRepository.MainCharacter = existingCharacter.WithAppliedData(character);
                _characterRepository.HasAvatar = true;
            }
            else if (_mapStateRepository.Characters.TryGetValue(character.ID, out var existingCharacter))
            {
                _mapStateRepository.Characters.Update(existingCharacter, existingCharacter.WithAppliedData(character));
            }
            else
            {
                _mapStateRepository.Characters.Add(character);
            }

            return true;
        }
    }
}
