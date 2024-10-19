using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Players
{
    /// <summary>
    /// Sent when a player is entering the map
    /// </summary>
    [AutoMappedType]
    public class PlayersAgreeHandler : InGameOnlyPacketHandler<PlayersAgreeServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _mapStateRepository;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Players;

        public override PacketAction Action => PacketAction.Agree;

        public PlayersAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICharacterRepository characterRepository,
                                   ICurrentMapStateRepository mapStateRepository,
                                   IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _mapStateRepository = mapStateRepository;
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(PlayersAgreeServerPacket packet)
        {
            foreach (var charInfo in packet.Nearby.Characters.Where(x => x.ByteSize >= 42))
            {
                var character = Character.FromNearby(charInfo);

                if (charInfo.WarpEffect.HasValue)
                {
                    foreach (var notifier in _effectNotifiers)
                        notifier.NotifyWarpEnterEffect(character.ID, charInfo.WarpEffect.Value);
                }

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
            }

            foreach (var npc in packet.Nearby.Npcs.Select(Domain.NPC.NPC.FromNearby))
            {
                if (_mapStateRepository.NPCs.TryGetValue(npc.Index, out var existing))
                {
                    _mapStateRepository.NPCs.Update(existing, npc);
                }
                else
                {
                    _mapStateRepository.NPCs.Add(npc);
                }
            }

            foreach (var item in packet.Nearby.Items.Select(MapItem.FromNearby))
            {
                if (_mapStateRepository.MapItems.TryGetValue(item.UniqueID, out var existing))
                {
                    _mapStateRepository.MapItems.Update(existing, item);
                }
                else
                {
                    _mapStateRepository.MapItems.Add(item);
                }
            }

            return true;
        }
    }
}
