using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System;
using System.Collections.Generic;
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
        private const int NPC_WALK_ACTION = 0;
        private const int NPC_ATTK_ACTION = 1;
        private const int NPC_TALK_ACTION = 2;

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
            var num255s = 0;
            while (packet.PeekByte() == byte.MaxValue)
            {
                num255s++;
                packet.ReadByte();
            }

            var index = packet.ReadChar();
            DomainNPC npc;
            try
            {
                npc = _currentMapStateRepository.NPCs.Single(n => n.Index == index);
            }
            catch (InvalidOperationException)
            {
                _currentMapStateRepository.UnknownNPCIndexes.Add(index);
                return true;
            }

            var updatedNpc = Option.None<DomainNPC>();
            switch (num255s)
            {
                case NPC_WALK_ACTION: HandleNPCWalk(packet, npc); break;
                case NPC_ATTK_ACTION: updatedNpc = Option.Some(HandleNPCAttack(packet, npc)); break;
                case NPC_TALK_ACTION: HandleNPCTalk(packet, npc); break;
                default: throw new MalformedPacketException("Unknown NPC action " + num255s + " specified in packet from server!", packet);
            }

            updatedNpc.MatchSome(n =>
            {
                _currentMapStateRepository.NPCs.Remove(npc);
                _currentMapStateRepository.NPCs.Add(n);
            });

            return true;
        }

        private void HandleNPCWalk(IPacket packet, DomainNPC npc)
        {
            //npc remove from view sets x/y to either 0,0 or 252,252 based on target coords
            var x = packet.ReadChar();
            var y = packet.ReadChar();
            var npcDirection = (EODirection)packet.ReadChar();
            if (packet.ReadBytes(3).Any(b => b != 255))
                throw new MalformedPacketException("Expected 3 bytes of value 0xFF in NPC_PLAYER packet for Walk action", packet);

            var updatedNPC = npc.WithDirection(npcDirection);
            updatedNPC = EnsureCorrectXAndY(updatedNPC, x, y);

            _currentMapStateRepository.NPCs.Remove(npc);
            _currentMapStateRepository.NPCs.Add(updatedNPC);

            foreach (var notifier in _npcAnimationNotifiers)
                notifier.StartNPCWalkAnimation(npc.Index);
        }

        private DomainNPC HandleNPCAttack(IPacket packet, DomainNPC npc)
        {
            var isDead = packet.ReadChar() == 2; //2 if target player is dead, 1 if alive
            var npcDirection = (EODirection)packet.ReadChar();
            var characterID = packet.ReadShort();
            var damageTaken = packet.ReadThree();
            var playerPercentHealth = packet.ReadThree();
            if (packet.ReadBytes(2).Any(b => b != 255))
                throw new MalformedPacketException("Expected 2 bytes of value 0xFF in NPC_PLAYER packet for Attack action", packet);

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
            else if (_currentMapStateRepository.Characters.ContainsKey(characterID))
            {
                var updatedCharacter = _currentMapStateRepository.Characters[characterID].WithDamage(damageTaken, isDead);
                _currentMapStateRepository.Characters[characterID] = updatedCharacter;

                foreach (var notifier in _otherCharacterNotifiers)
                    notifier.OtherCharacterTakeDamage(characterID, playerPercentHealth, damageTaken, isHeal: false);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(characterID);
            }

            foreach (var notifier in _npcAnimationNotifiers)
                notifier.StartNPCAttackAnimation(npc.Index);

            return npc.WithDirection(npcDirection);
        }

        private void HandleNPCTalk(IPacket packet, DomainNPC npc)
        {
            var messageLength = packet.ReadChar();
            var message = packet.ReadString(messageLength);

            var npcData = _enfFileProvider.ENFFile[npc.ID];

            var chatData = new ChatData(ChatTab.Local, npcData.Name, message, ChatIcon.Note);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);

            foreach (var notifier in _npcAnimationNotifiers)
                notifier.ShowNPCSpeechBubble(npc.Index, message);
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
