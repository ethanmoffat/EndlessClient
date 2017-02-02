// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
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

namespace EOLib.PacketHandlers
{
    public class NPCActionHandler : InGameOnlyPacketHandler
    {
        private const int NPC_WALK_ACTION = 0;
        private const int NPC_ATTK_ACTION = 1;
        private const int NPC_TALK_ACTION = 2;

        private readonly ICharacterRepository _characterRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<INPCAnimationNotifier> _npcAnimationNotifiers;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterNotifiers;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterNotifiers;

        public override PacketFamily Family => PacketFamily.NPC;

        public override PacketAction Action => PacketAction.Player;

        public NPCActionHandler(IPlayerInfoProvider playerInfoProvider,
                                ICurrentMapStateRepository currentMapStateRepository,
                                ICharacterRepository characterRepository,
                                IChatRepository chatRepository,
                                IENFFileProvider enfFileProvider,
                                IEnumerable<INPCAnimationNotifier> npcAnimationNotifiers,
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
            INPC npc;
            try
            {
                npc = _currentMapStateRepository.NPCs.Single(n => n.Index == index);
            }
            catch (InvalidOperationException) { return false; }

            var updatedNpc = Optional<INPC>.Empty;
            switch (num255s)
            {
                case NPC_WALK_ACTION: HandleNPCWalk(packet, npc); break;
                case NPC_ATTK_ACTION: updatedNpc = HandleNPCAttack(packet, npc); break;
                case NPC_TALK_ACTION: HandleNPCTalk(packet, npc); break;
                default: throw new MalformedPacketException("Unknown NPC action " + num255s + " specified in packet from server!", packet);
            }

            if (updatedNpc.HasValue)
            {
                _currentMapStateRepository.NPCs.Remove(npc);
                _currentMapStateRepository.NPCs.Add(updatedNpc.Value);
            }

            return true;
        }

        private void HandleNPCWalk(IPacket packet, INPC npc)
        {
            //npc remove from view sets x/y to either 0,0 or 252,252 based on target coords
            var x = packet.ReadChar();
            var y = packet.ReadChar();
            var npcDirection = (EODirection) packet.ReadChar();
            if (packet.ReadBytes(3).Any(b => b != 255))
                throw new MalformedPacketException("Expected 3 bytes of value 0xFF in NPC_PLAYER packet for Walk action", packet);

            var updatedNPC = npc.WithDirection(npcDirection);
            updatedNPC = EnsureCorrectXAndY(updatedNPC, x, y);

            _currentMapStateRepository.NPCs.Remove(npc);
            _currentMapStateRepository.NPCs.Add(updatedNPC);

            foreach (var notifier in _npcAnimationNotifiers)
                notifier.StartNPCWalkAnimation(npc.Index);
        }

        private Optional<INPC> HandleNPCAttack(IPacket packet, INPC npc)
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
                stats = stats.WithNewStat(CharacterStat.HP, (short)Math.Max(stats[CharacterStat.HP] - damageTaken, 0));

                var props = characterToUpdate.RenderProperties;
                if (isDead)
                    props = props.WithDead();

                _characterRepository.MainCharacter = characterToUpdate.WithStats(stats).WithRenderProperties(props);

                foreach (var notifier in _mainCharacterNotifiers)
                    notifier.NotifyTakeDamage(damageTaken, playerPercentHealth);
            }
            else
            {
                var characterToUpdate = _currentMapStateRepository.Characters.Single(x => x.ID == characterID);

                var stats = characterToUpdate.Stats;
                stats = stats.WithNewStat(CharacterStat.HP, (short) Math.Max(stats[CharacterStat.HP] - damageTaken, 0));

                var props = characterToUpdate.RenderProperties;
                if (isDead)
                    props = props.WithDead();

                var updatedCharacter = characterToUpdate.WithStats(stats).WithRenderProperties(props);
                _currentMapStateRepository.Characters.Remove(characterToUpdate);
                _currentMapStateRepository.Characters.Add(updatedCharacter);

                foreach (var notifier in _otherCharacterNotifiers)
                    notifier.OtherCharacterTakeDamage(characterID, playerPercentHealth, damageTaken);
            }

            foreach (var notifier in _npcAnimationNotifiers)
                notifier.StartNPCAttackAnimation(npc.Index);

            return new Optional<INPC>(npc.WithDirection(npcDirection));
        }

        private void HandleNPCTalk(IPacket packet, INPC npc)
        {
            var messageLength = packet.ReadChar();
            var message = packet.ReadString(messageLength);

            var npcData = _enfFileProvider.ENFFile[npc.ID];

            var chatData = new ChatData(npcData.Name, message, ChatIcon.Note);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);
        }

        private static INPC EnsureCorrectXAndY(INPC npc, byte destinationX, byte destinationY)
        {
            var opposite = npc.Direction.Opposite();
            var tempNPC = npc
                .WithDirection(opposite)
                .WithX(destinationX)
                .WithY(destinationY);
            return npc
                .WithX((byte)tempNPC.GetDestinationX())
                .WithY((byte)tempNPC.GetDestinationY());
        }
    }
}
