using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Domain.NPC;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DomainNPC = EOLib.Domain.NPC.NPC;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Sent when an NPC does something (walk/attack/talk)
    /// </summary>
    [AutoMappedType]
    public class NPCPlayerHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<INPCActionNotifier> _npcAnimationNotifiers;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterNotifiers;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterNotifiers;

        public override PacketFamily Family => PacketFamily.NPC;

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

        public override bool HandlePacket(IPacket packet)
        {
            var chunks = new List<IPacket>();
            while (packet.ReadPosition < packet.Length)
            {
                var data = packet.RawData.Skip(packet.ReadPosition).TakeWhile(x => x != 255).ToArray();
                packet.Seek(data.Length, SeekOrigin.Current);
                if (packet.ReadPosition < packet.Length)
                    packet.ReadByte();

                chunks.Add(new PacketBuilder(packet.Family, packet.Action).AddBytes(data).Build());
            }

            if (chunks.Count < 3 || chunks.Count > 4)
                throw new MalformedPacketException($"Expected 3 or 4 chunks in NPC_PLAYER packet, got {chunks.Count}", packet);

            HandleNPCWalk(chunks[0]);
            HandleNPCAttack(chunks[1]);
            HandleNPCTalk(chunks[2]);

            if (chunks.Count > 3)
            {
                var hp = chunks[3].ReadShort();
                var tp = chunks[3].ReadShort();

                var stats = _characterRepository.MainCharacter.Stats
                    .WithNewStat(CharacterStat.HP, hp)
                    .WithNewStat(CharacterStat.TP, tp);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
            }

            return true;
        }

        private void HandleNPCWalk(IPacket packet)
        {
            while (packet.ReadPosition < packet.Length)
            {
                var index = packet.ReadChar();
                var x = packet.ReadChar();
                var y = packet.ReadChar();
                var npcDirection = (EODirection)packet.ReadChar();

                var npc = GetNPC(index);
                npc.Match(
                    some: n =>
                    {
                        var updated = n.WithDirection(npcDirection);
                        updated = EnsureCorrectXAndY(updated, x, y);
                        ReplaceNPC(n, updated);

                        foreach (var notifier in _npcAnimationNotifiers)
                            notifier.StartNPCWalkAnimation(n.Index);
                    },
                    none: () => _currentMapStateRepository.UnknownNPCIndexes.Add(index));
            }
        }

        private void HandleNPCAttack(IPacket packet)
        {
            // note: eoserv incorrectly sends playerPercentHealth as a three byte number. GameServer sends a single char.
            const int DATA_LENGTH = 9;

            while (packet.ReadPosition + DATA_LENGTH < packet.Length)
            {
                var index = packet.ReadChar();
                var isDead = packet.ReadChar() == 2; // 2 if target player is dead, 1 if alive
                var npcDirection = (EODirection)packet.ReadChar();
                var characterID = packet.ReadShort();
                var damageTaken = packet.ReadThree();
                var playerPercentHealth = packet.ReadChar();

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
                else
                {
                    _currentMapStateRepository.UnknownPlayerIDs.Add(characterID);
                }

                var npc = GetNPC(index);
                npc.Match(
                    some: n =>
                    {
                        var updated = n.WithDirection(npcDirection);
                        ReplaceNPC(n, updated);

                        foreach (var notifier in _npcAnimationNotifiers)
                            notifier.StartNPCAttackAnimation(index);
                    },
                    none: () => _currentMapStateRepository.UnknownNPCIndexes.Add(index));
            }
        }

        private void HandleNPCTalk(IPacket packet)
        {
            while (packet.ReadPosition < packet.Length)
            {
                var index = packet.ReadChar();
                var messageLength = packet.ReadChar();
                var message = packet.ReadString(messageLength);

                var npc = GetNPC(index);
                npc.Match(
                    some: n =>
                    {
                        var npcData = _enfFileProvider.ENFFile[n.ID];

                        var chatData = new ChatData(ChatTab.Local, npcData.Name, message, ChatIcon.Note);
                        _chatRepository.AllChat[ChatTab.Local].Add(chatData);

                        foreach (var notifier in _npcAnimationNotifiers)
                            notifier.ShowNPCSpeechBubble(index, message);
                    },
                    none: () => _currentMapStateRepository.UnknownNPCIndexes.Add(index));
            }
        }

        private Option<DomainNPC> GetNPC(int index)
        {
            return _currentMapStateRepository.NPCs.SingleOrNone(n => n.Index == index);
        }

        private void ReplaceNPC(DomainNPC npc, DomainNPC updatedNPC)
        {
            _currentMapStateRepository.NPCs.Remove(npc);
            _currentMapStateRepository.NPCs.Add(updatedNPC);
        }

        private static DomainNPC EnsureCorrectXAndY(DomainNPC npc, int destinationX, int destinationY)
        {
            var opposite = npc.Direction.Opposite();
            var tempNPC = npc
                .WithDirection(opposite)
                .WithX(destinationX)
                .WithY(destinationY);
            return npc
                .WithX(tempNPC.GetDestinationX())
                .WithY(tempNPC.GetDestinationY());
        }
    }
}
