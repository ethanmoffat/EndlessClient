using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Sent when an NPC does something (walk/attack/talk)
    /// </summary>
    [AutoMappedType]
    public class NPCPlayerHandler : InGameOnlyPacketHandler<NpcPlayerServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<INPCActionNotifier> _npcAnimationNotifiers;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterNotifiers;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterNotifiers;

        public override PacketFamily Family => PacketFamily.Npc;

        public override PacketAction Action => PacketAction.Player;

        public NPCPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                ICurrentMapStateRepository currentMapStateRepository,
                                ICharacterRepository characterRepository,
                                IChatRepository chatRepository,
                                IENFFileProvider enfFileProvider,
                                IEnumerable<INPCActionNotifier> npcAnimationNotifiers,
                                IEnumerable<IMainCharacterEventNotifier> mainCharacterNotifiers,
                                IEnumerable<IOtherCharacterEventNotifier> otherCharacterNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _characterRepository = characterRepository;
            _chatRepository = chatRepository;
            _enfFileProvider = enfFileProvider;
            _npcAnimationNotifiers = npcAnimationNotifiers;
            _mainCharacterNotifiers = mainCharacterNotifiers;
            _otherCharacterNotifiers = otherCharacterNotifiers;
        }

        public override bool HandlePacket(NpcPlayerServerPacket packet)
        {
            HandleNPCWalk(packet.Positions);
            HandleNPCAttack(packet.Attacks);
            HandleNPCTalk(packet.Chats);

            var stats = _characterRepository.MainCharacter.Stats;
            if (packet.Hp.HasValue)
            {
                stats = stats.WithNewStat(CharacterStat.HP, packet.Hp.Value);
            }
            if (packet.Tp.HasValue)
            {
                stats = stats.WithNewStat(CharacterStat.TP, packet.Tp.Value);
            }
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }

        private void HandleNPCWalk(IReadOnlyList<NpcUpdatePosition> positions)
        {
            foreach (var position in positions)
            {
                if (_currentMapStateRepository.NPCs.TryGetValue(position.NpcIndex, out var npc))
                {
                    foreach (var notifier in _npcAnimationNotifiers)
                        notifier.StartNPCWalkAnimation(npc.Index, position.Coords.ToCoordinate(), (EODirection)position.Direction);
                }
                else
                {
                    _currentMapStateRepository.UnknownNPCIndexes.Add(position.NpcIndex);
                }
            }
        }

        private void HandleNPCAttack(IReadOnlyList<NpcUpdateAttack> attacks)
        {
            foreach (var attack in attacks)
            {
                var index = attack.NpcIndex;
                var isDead = attack.Killed == PlayerKilledState.Killed;
                var npcDirection = (EODirection)attack.Direction;
                var characterID = attack.PlayerId;
                var damageTaken = attack.Damage;
                var playerPercentHealth = attack.HpPercentage;

                if (characterID == _characterRepository.MainCharacter.ID)
                {
                    var characterToUpdate = _characterRepository.MainCharacter;

                    var stats = characterToUpdate.Stats;
                    stats = stats.WithNewStat(CharacterStat.HP, Math.Max(stats[CharacterStat.HP] - damageTaken, 0));

                    var props = characterToUpdate.RenderProperties.WithIsDead(isDead);
                    _characterRepository.MainCharacter = characterToUpdate.WithStats(stats).WithRenderProperties(props);

                    foreach (var notifier in _mainCharacterNotifiers)
                        notifier.NotifyTakeDamage(damageTaken, playerPercentHealth, isHeal: false);
                }
                else if (_currentMapStateRepository.Characters.TryGetValue(characterID, out var character))
                {
                    var updatedCharacter = character.WithDamage(damageTaken, isDead);
                    _currentMapStateRepository.Characters.Update(character, updatedCharacter);

                    foreach (var notifier in _otherCharacterNotifiers)
                        notifier.OtherCharacterTakeDamage(characterID, playerPercentHealth, damageTaken, isHeal: false);
                }
                else if (characterID > 0)
                {
                    _currentMapStateRepository.UnknownPlayerIDs.Add(characterID);
                }

                if (_currentMapStateRepository.NPCs.TryGetValue(index, out var npc))
                {
                    foreach (var notifier in _npcAnimationNotifiers)
                        notifier.StartNPCAttackAnimation(index, (EODirection)attack.Direction);
                }
                else
                {
                    _currentMapStateRepository.UnknownNPCIndexes.Add(index);
                }
            }
        }

        private void HandleNPCTalk(IReadOnlyList<NpcUpdateChat> chats)
        {
            foreach (var chat in chats)
            {
                if (_currentMapStateRepository.NPCs.TryGetValue(chat.NpcIndex, out var npc))
                {
                    var npcData = _enfFileProvider.ENFFile[npc.ID];

                    var chatData = new ChatData(ChatTab.Local, npcData.Name, chat.Message, ChatIcon.Note, filter: false);
                    _chatRepository.AllChat[ChatTab.Local].Add(chatData);

                    foreach (var notifier in _npcAnimationNotifiers)
                        notifier.ShowNPCSpeechBubble(chat.NpcIndex, chat.Message);
                }
                else
                {
                    _currentMapStateRepository.UnknownNPCIndexes.Add(chat.NpcIndex);
                }
            }
        }
    }
}
