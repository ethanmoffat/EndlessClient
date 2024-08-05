using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Arena
{
    /// <summary>
    /// Arena kill message
    /// </summary>
    [AutoMappedType]
    public class ArenaSpecHandler : InGameOnlyPacketHandler<ArenaSpecServerPacket>
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

        public override bool HandlePacket(ArenaSpecServerPacket packet)
        {
            if (packet.PlayerId == _characterRepository.MainCharacter.ID)
            {
                var rp = _characterRepository.MainCharacter.RenderProperties.WithDirection((EODirection)packet.Direction);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(rp);
            }
            else if (_currentMapStateRepository.Characters.ContainsKey(packet.PlayerId))
            {
                var character = _currentMapStateRepository.Characters[packet.PlayerId];
                var rp = character.RenderProperties.WithDirection((EODirection)packet.Direction);
                var newCharacter = character.WithRenderProperties(rp);
                _currentMapStateRepository.Characters.Update(character, newCharacter);
            }
            else if (packet.PlayerId > 0)
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(packet.PlayerId);
            }

            foreach (var notifier in _arenaNotifiers)
            {
                notifier.NotifyArenaKill(packet.KillsCount, packet.KillerName, packet.VictimName);
            }

            return true;
        }
    }
}