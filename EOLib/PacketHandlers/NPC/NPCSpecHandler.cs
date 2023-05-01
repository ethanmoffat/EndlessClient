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

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Sent when an NPC dies to a weapon
    /// </summary>
    [AutoMappedType]
    public class NPCSpecHandler : InGameOnlyPacketHandler
    {
        protected readonly ICurrentMapStateRepository _currentMapStateRepository;
        protected readonly ICharacterRepository _characterRepository;
        private readonly ICharacterSessionRepository _characterSessionRepository;
        private readonly IEnumerable<INPCActionNotifier> _npcActionNotifiers;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _otherCharacterAnimationNotifiers;

        public override PacketFamily Family => PacketFamily.NPC;

        public override PacketAction Action => PacketAction.Spec;

        public NPCSpecHandler(IPlayerInfoProvider playerInfoProvider,
                              ICurrentMapStateRepository currentMapStateRepository,
                              ICharacterRepository characterRepository,
                              ICharacterSessionRepository characterSessionRepository,
                              IEnumerable<INPCActionNotifier> npcActionNotifiers,
                              IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                              IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _characterRepository = characterRepository;
            _characterSessionRepository = characterSessionRepository;
            _npcActionNotifiers = npcActionNotifiers;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
            _otherCharacterAnimationNotifiers = otherCharacterAnimationNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var spellId = packet.Family
                .SomeWhen(f => f == PacketFamily.Cast)
                .Map(f => packet.ReadShort());

            var fromPlayerId = packet.ReadShort(); //player that is protecting the item
            var playerDirection = (EODirection)packet.ReadChar();
            if (fromPlayerId > 0)
                UpdatePlayerDirection(fromPlayerId, playerDirection);

            var deadNPCIndex = packet.ReadShort();

            //packet is removing the NPC from view due to out of range of character
            if (packet.ReadPosition == packet.Length)
            {
                RemoveNPCFromView(deadNPCIndex, fromPlayerId, spellId, damage: Option.None<int>(), showDeathAnimation: false);
                return true;
            }

            var droppedItemUID = packet.ReadShort();
            var droppedItemID = packet.ReadShort();
            var x = packet.ReadChar();
            var y = packet.ReadChar();
            var droppedAmount = packet.ReadInt();

            var damageDoneToNPC = packet.ReadThree();
            RemoveNPCFromView(deadNPCIndex, fromPlayerId, spellId, Option.Some(damageDoneToNPC), showDeathAnimation: true);

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

                _characterSessionRepository.LastKillExp = expDifference;
                if (expDifference > _characterSessionRepository.BestKillExp)
                    _characterSessionRepository.BestKillExp = expDifference;
                _characterSessionRepository.TodayTotalExp += Convert.ToUInt64(Math.Max(expDifference, 0));
            }

            if (droppedItemID > 0)
                ShowDroppedItem(fromPlayerId, droppedItemUID, droppedItemID, x, y, droppedAmount);

            spellId.MatchSome(_ =>
            {
                foreach (var notifier in _otherCharacterAnimationNotifiers)
                    notifier.NotifyTargetNpcSpellCast(fromPlayerId);
            });

            return true;
        }

        private void RemoveNPCFromView(int deadNPCIndex, int playerId, Option<int> spellId, Option<int> damage, bool showDeathAnimation)
        {
            foreach (var notifier in _npcActionNotifiers)
                notifier.RemoveNPCFromView(deadNPCIndex, playerId, spellId, damage, showDeathAnimation);

            _currentMapStateRepository.NPCs.RemoveWhere(npc => npc.Index == deadNPCIndex);
        }

        private void UpdatePlayerDirection(int playerID, EODirection playerDirection)
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

        private void ShowDroppedItem(int playerID, int droppedItemUID, int droppedItemID, int x, int y, int droppedAmount)
        {
            var mapItem = new MapItem(droppedItemUID, droppedItemID, x, y, droppedAmount)
                .WithIsNPCDrop(true)
                .WithDropTime(Option.Some(DateTime.Now))
                .WithOwningPlayerID(Option.Some(playerID));

            _currentMapStateRepository.MapItems.RemoveWhere(item => item.UniqueID == droppedItemUID);
            _currentMapStateRepository.MapItems.Add(mapItem);

            foreach (var notifier in _npcActionNotifiers)
                notifier.NPCDropItem(mapItem);
        }
    }
}
