// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    public class NPCLeaveMapHandler : InGameOnlyPacketHandler
    {
        protected readonly ICurrentMapStateRepository _currentMapStateRepository;
        protected readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<INPCAnimationNotifier> _npcAnimationNotifiers;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.NPC;

        public override PacketAction Action => PacketAction.Spec;

        public NPCLeaveMapHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICurrentMapStateRepository currentMapStateRepository,
                                  ICharacterRepository characterRepository,
                                  IEnumerable<INPCAnimationNotifier> npcAnimationNotifiers,
                                  IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _characterRepository = characterRepository;
            _npcAnimationNotifiers = npcAnimationNotifiers;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var spellID = Optional<short>.Empty;
            if (packet.Family == PacketFamily.Cast)
                spellID = packet.ReadShort();

            var playerID = packet.ReadShort(); //player that is protecting the item
            var playerDirection = (EODirection)packet.ReadChar();
            if (playerID > 0)
                UpdatePlayerDirection(playerID, playerDirection);

            var deadNPCIndex = packet.ReadShort();
            if (spellID.HasValue)
            {
                //todo: render spell ID on deadNPCIndex (when spells are supported)
            }

            //packet is removing the NPC from view due to out of range of character
            if (packet.ReadPosition == packet.Length)
            {
                RemoveNPCFromView(deadNPCIndex, false);
                return true;
            }

            //packet is removing NPC from view due to dying
            RemoveNPCFromView(deadNPCIndex, true);

            var droppedItemUID = packet.ReadShort();
            var droppedItemID = packet.ReadShort();
            var x = packet.ReadChar();
            var y = packet.ReadChar();
            var droppedAmount = packet.ReadInt();

            var damageDoneToNPC = packet.ReadThree();
            //todo: show damage done to NPC (when damage counters are supported)

            if (packet.Family == PacketFamily.Cast)
            {
                var characterTPRemaining = packet.ReadShort();
                UpdateCharacterStat(CharacterStat.TP, characterTPRemaining);
            }

            if (packet.ReadPosition != packet.Length)
            {
                var playerExp = packet.ReadInt();
                var expDifference = playerExp - _characterRepository.MainCharacter.Stats[CharacterStat.Experience];
                foreach (var notifier in _mainCharacterEventNotifiers)
                    notifier.NotifyGainedExp(expDifference);

                UpdateCharacterStat(CharacterStat.Experience, playerExp);
                //todo: update last kill, best kill, and today exp
            }

            if (droppedItemID > 0)
                ShowDroppedItem(playerID, droppedItemUID, droppedItemID, x, y, droppedAmount);

            return true;
        }

        private void RemoveNPCFromView(short deadNPCIndex, bool showDeathAnimation)
        {
            foreach (var notifier in _npcAnimationNotifiers)
                notifier.RemoveNPCFromView(deadNPCIndex, showDeathAnimation);

            _currentMapStateRepository.NPCs = _currentMapStateRepository.NPCs
                .Where(npc => npc.Index != deadNPCIndex)
                .ToList();
        }

        private void UpdatePlayerDirection(short playerID, EODirection playerDirection)
        {
            if (playerID == _characterRepository.MainCharacter.ID)
            {
                var updatedRenderProps = _characterRepository.MainCharacter
                    .RenderProperties.WithDirection(playerDirection);
                var updatedCharacter = _characterRepository.MainCharacter
                    .WithRenderProperties(updatedRenderProps);

                _characterRepository.MainCharacter = updatedCharacter;
            }
            else
            {
                var character = _currentMapStateRepository.Characters.Single(x => x.ID == playerID);
                var updatedRenderProps = character.RenderProperties.WithDirection(playerDirection);
                var updatedCharacter = character.WithRenderProperties(updatedRenderProps);

                _currentMapStateRepository.Characters.Remove(character);
                _currentMapStateRepository.Characters.Add(updatedCharacter);
            }
        }

        private void UpdateCharacterStat(CharacterStat whichStat, int statValue)
        {
            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(whichStat, statValue);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
        }

        private void ShowDroppedItem(short playerID, short droppedItemUID, short droppedItemID, byte x, byte y, int droppedAmount)
        {
            IItem mapItem = new Item(droppedItemUID, droppedItemID, x, y);
            mapItem = mapItem.WithAmount(droppedAmount)
                .WithIsNPCDrop(true)
                .WithDropTime(DateTime.Now)
                .WithOwningPlayerID(playerID);

            _currentMapStateRepository.MapItems.RemoveAll(item => item.UniqueID == droppedItemUID);
            _currentMapStateRepository.MapItems.Add(mapItem);
        }
    }

    /// <summary>
    /// This is handled the same way as the NPC_SPEC packet. There is some additional special handling 
    /// that is done from NPCLeaveMapHandler.HandlePlacket (see if packet.Family == PacketFamily.Cast) blocks
    /// </summary>
    public class NPCDieFromSpellCastHandler : NPCLeaveMapHandler
    {
        public override PacketFamily Family => PacketFamily.Cast;

        public override PacketAction Action => PacketAction.Spec;

        public NPCDieFromSpellCastHandler(IPlayerInfoProvider playerInfoProvider,
                                          ICurrentMapStateRepository currentMapStateRepository,
                                          ICharacterRepository characterRepository,
                                          IEnumerable<INPCAnimationNotifier> npcAnimationNotifiers,
                                          IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository,
                   npcAnimationNotifiers, mainCharacterEventNotifiers) { }
    }
}
