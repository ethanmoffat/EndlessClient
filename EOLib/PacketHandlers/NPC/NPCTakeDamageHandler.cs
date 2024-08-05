using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Base type for NPC taking damage
    /// </summary>
    public abstract class NPCTakeDamageHandler<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<INPCActionNotifier> _npcNotifiers;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _otherCharacterAnimationNotifiers;

        public override PacketAction Action => PacketAction.Reply;

        public NPCTakeDamageHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICharacterRepository characterRepository,
                                    ICurrentMapStateRepository currentMapStateRepository,
                                    IEnumerable<INPCActionNotifier> npcNotifiers,
                                    IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _npcNotifiers = npcNotifiers;
            _otherCharacterAnimationNotifiers = otherCharacterAnimationNotifiers;
        }

        protected void Handle(int fromPlayerId, EODirection fromDirection,
            int npcIndex, int damageToNpc, int npcPctHealth,
            int? spellId = null, int? fromTp = null)
        {
            if (fromPlayerId == _characterRepository.MainCharacter.ID)
            {
                var renderProps = _characterRepository.MainCharacter.RenderProperties;
                var character = _characterRepository.MainCharacter.WithRenderProperties(renderProps.WithDirection(fromDirection));

                if (fromTp.HasValue)
                {
                    var stats = _characterRepository.MainCharacter.Stats;
                    character = character.WithStats(stats.WithNewStat(CharacterStat.TP, fromTp.Value));
                }

                _characterRepository.MainCharacter = character;
            }
            else if (_currentMapStateRepository.Characters.TryGetValue(fromPlayerId, out var character))
            {
                var renderProps = character.RenderProperties.WithDirection(fromDirection);
                var updatedCharacter = character.WithRenderProperties(renderProps);
                _currentMapStateRepository.Characters.Update(character, updatedCharacter);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(fromPlayerId);
            }

            var spellIdOptional = spellId.HasValue
                ? Option.Some(spellId.Value)
                : Option.None<int>();

            // todo: this has the potential to bug out if the opponent ID is never reset and the player dies/leaves
            if (_currentMapStateRepository.NPCs.TryGetValue(npcIndex, out var npc))
            {
                var newNpc = npc.WithOpponentID(Option.Some(fromPlayerId));
                _currentMapStateRepository.NPCs.Update(npc, newNpc);

                foreach (var notifier in _npcNotifiers)
                    notifier.NPCTakeDamage(npcIndex, fromPlayerId, damageToNpc, npcPctHealth, spellIdOptional);
            }
            else
            {
                _currentMapStateRepository.UnknownNPCIndexes.Add(npcIndex);
            }

            spellIdOptional.MatchSome(_ =>
            {
                foreach (var notifier in _otherCharacterAnimationNotifiers)
                    notifier.NotifyTargetNpcSpellCast(fromPlayerId);
            });
        }
    }
}