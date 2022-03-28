using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System;
using System.Collections.Generic;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class NPCLeaveMapHandler : InGameOnlyPacketHandler
    {
        protected readonly ICurrentMapStateRepository _currentMapStateRepository;
        protected readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<INPCActionNotifier> _npcAnimationNotifiers;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.NPC;

        public override PacketAction Action => PacketAction.Spec;

        public NPCLeaveMapHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICurrentMapStateRepository currentMapStateRepository,
                                  ICharacterRepository characterRepository,
                                  IEnumerable<INPCActionNotifier> npcAnimationNotifiers,
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
            var spellID = packet.Family
                .SomeWhen(f => f == PacketFamily.Cast)
                .Map(f => packet.ReadShort());

            var playerID = packet.ReadShort(); //player that is protecting the item
            var playerDirection = (EODirection)packet.ReadChar();
            if (playerID > 0)
                UpdatePlayerDirection(playerID, playerDirection);

            var deadNPCIndex = packet.ReadShort();

            //packet is removing the NPC from view due to out of range of character
            if (packet.ReadPosition == packet.Length)
            {
                RemoveNPCFromView(deadNPCIndex, playerID, spellID, damage: Option.None<int>(), showDeathAnimation: false);
                return true;
            }

            var droppedItemUID = packet.ReadShort();
            var droppedItemID = packet.ReadShort();
            var x = packet.ReadChar();
            var y = packet.ReadChar();
            var droppedAmount = packet.ReadInt();

            var damageDoneToNPC = packet.ReadThree();
            RemoveNPCFromView(deadNPCIndex, playerID, spellID, Option.Some(damageDoneToNPC), showDeathAnimation: true);

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

        private void RemoveNPCFromView(short deadNPCIndex, int playerId, Option<short> spellId, Option<int> damage, bool showDeathAnimation)
        {
            foreach (var notifier in _npcAnimationNotifiers)
                notifier.RemoveNPCFromView(deadNPCIndex, playerId, spellId, damage, showDeathAnimation);

            _currentMapStateRepository.NPCs.RemoveWhere(npc => npc.Index == deadNPCIndex);
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
            else if (_currentMapStateRepository.Characters.ContainsKey(playerID))
            {
                var updatedRenderProps = _currentMapStateRepository.Characters[playerID].RenderProperties.WithDirection(playerDirection);
                var updatedCharacter = _currentMapStateRepository.Characters[playerID].WithRenderProperties(updatedRenderProps);
                _currentMapStateRepository.Characters[playerID] = updatedCharacter;
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerID);
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

            _currentMapStateRepository.MapItems.RemoveWhere(item => item.UniqueID == droppedItemUID);
            _currentMapStateRepository.MapItems.Add(mapItem);
        }
    }

    /// <summary>
    /// This is handled the same way as the NPC_SPEC packet. There is some additional special handling 
    /// that is done from NPCLeaveMapHandler.HandlePlacket (see if packet.Family == PacketFamily.Cast) blocks
    /// </summary>
    [AutoMappedType]
    public class NPCDieFromSpellCastHandler : NPCLeaveMapHandler
    {
        public override PacketFamily Family => PacketFamily.Cast;

        public override PacketAction Action => PacketAction.Spec;

        public NPCDieFromSpellCastHandler(IPlayerInfoProvider playerInfoProvider,
                                          ICurrentMapStateRepository currentMapStateRepository,
                                          ICharacterRepository characterRepository,
                                          IEnumerable<INPCActionNotifier> npcAnimationNotifiers,
                                          IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository,
                   npcAnimationNotifiers, mainCharacterEventNotifiers) { }
    }
}
