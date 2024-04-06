using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Arena
{
    /// <summary>
    /// Arena kill message
    /// </summary>
    [AutoMappedType]
    public class ArenaSpecHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IArenaNotifier> _arenaNotifiers;

        public override PacketFamily Family => PacketFamily.Arena;

        public override PacketAction Action => PacketAction.Spec;

        public ArenaSpecHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository,
                                IEnumerable<IArenaNotifier> arenaNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _arenaNotifiers = arenaNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerId = packet.ReadShort();
            packet.ReadByte();

            var playerDirection = (EODirection)packet.ReadChar();
            packet.ReadByte();

            if (playerId == _characterRepository.MainCharacter.ID)
            {
                var rp = _characterRepository.MainCharacter.RenderProperties.WithDirection(playerDirection);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(rp);
            }
            else if (_currentMapStateRepository.Characters.ContainsKey(playerId))
            {
                var character = _currentMapStateRepository.Characters[playerId];
                var rp = character.RenderProperties.WithDirection(playerDirection);
                var newCharacter = character.WithRenderProperties(rp);
                _currentMapStateRepository.Characters.Update(character, newCharacter);
            }
            else if (playerId > 0)
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerId);
            }

            var killsCount = packet.ReadInt();
            packet.ReadByte();

            var killerName = packet.ReadBreakString();
            var victimName = packet.ReadBreakString();

            foreach (var  notifier in _arenaNotifiers)
            {
                notifier.NotifyArenaKill(killsCount, killerName, victimName);
            }

            return true;
        }
    }
}
