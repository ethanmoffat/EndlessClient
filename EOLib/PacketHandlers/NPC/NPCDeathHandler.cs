using System;
using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Skill;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.NPC
{
    public abstract class NPCDeathHandler<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
    {
        protected readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICharacterSessionRepository _characterSessionRepository;
        private readonly IEnumerable<INPCActionNotifier> _npcActionNotifiers;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _otherCharacterAnimationNotifiers;

        protected NPCDeathHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICharacterRepository characterRepository,
                                  ICurrentMapStateRepository currentMapStateRepository,
                                  ICharacterSessionRepository characterSessionRepository,
                                  IEnumerable<INPCActionNotifier> npcActionNotifiers,
                                  IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                                  IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _characterSessionRepository = characterSessionRepository;
            _npcActionNotifiers = npcActionNotifiers;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
            _otherCharacterAnimationNotifiers = otherCharacterAnimationNotifiers;
        }

        protected void DeathWorkflow(NpcKilledData killData, int? experience)
        {
            DeathWorkflowSpell(killData, experience, Option.None<(int, int)>());
        }

        protected void DeathWorkflowSpell(NpcKilledData killData, int? experience, Option<(int SpellId, int CasterTp)> castProperties)
        {
            var optionalDamage = killData.Damage.SomeWhen(x => x > 0);
            // note: this replicates prior functionality implemented based on EOSERV
            // EOSERV sends a 5-byte packet in Npc::RemoveFromView to kill without the animation
            var showDeathAnimation = (killData.ByteSize > 7 && castProperties.HasValue) || killData.ByteSize > 5;
            RemoveNPCFromView(killData.NpcIndex, killData.KillerId, castProperties.Map(x => x.SpellId), optionalDamage, showDeathAnimation);

            if (killData.KillerId > 0)
            {
                UpdatePlayerDirection(killData.KillerId, (EODirection)killData.KillerDirection);
            }

            if (killData.DropId > 0)
            {
                ShowDroppedItem(killData);
            }

            if (experience.HasValue)
            {
                UpdatePlayerExperience(experience.Value);
            }

            castProperties.MatchSome(x =>
            {
                UpdateCharacterStat(CharacterStat.TP, x.CasterTp);
                NotifySpellCast(killData.KillerId);
            });
        }

        protected void UpdatePlayerDirection(int playerID, EODirection playerDirection)
        {
            if (playerID == _characterRepository.MainCharacter.ID)
            {
                var updatedRenderProps = _characterRepository.MainCharacter.RenderProperties.WithDirection(playerDirection);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(updatedRenderProps);
            }
            else if (_currentMapStateRepository.Characters.TryGetValue(playerID, out var character))
            {
                var updatedRenderProps = character.RenderProperties.WithDirection(playerDirection);
                _currentMapStateRepository.Characters.Update(character, character.WithRenderProperties(updatedRenderProps));
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerID);
            }
        }

        protected void RemoveNPCFromView(int deadNPCIndex, int playerId, Option<int> spellId, Option<int> damage, bool showDeathAnimation)
        {
            foreach (var notifier in _npcActionNotifiers)
                notifier.RemoveNPCFromView(deadNPCIndex, playerId, spellId, damage, showDeathAnimation);

            if (_currentMapStateRepository.NPCs.TryGetValue(deadNPCIndex, out var npc))
                _currentMapStateRepository.NPCs.Remove(npc);
        }

        protected void UpdateCharacterStat(CharacterStat whichStat, int statValue)
        {
            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(whichStat, statValue);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
        }

        protected void UpdatePlayerExperience(int experienceValue)
        {
            var expDifference = experienceValue - _characterRepository.MainCharacter.Stats[CharacterStat.Experience];

            if (expDifference > 0)
            {
                foreach (var notifier in _mainCharacterEventNotifiers)
                    notifier.NotifyGainedExp(expDifference);

                UpdateCharacterStat(CharacterStat.Experience, experienceValue);

                _characterSessionRepository.LastKillExp = expDifference;
                if (expDifference > _characterSessionRepository.BestKillExp)
                    _characterSessionRepository.BestKillExp = expDifference;
                _characterSessionRepository.TodayTotalExp += Convert.ToUInt64(Math.Max(expDifference, 0));
            }
        }

        protected void ApplyStats(LevelUpStats levelUpStats)
        {
            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Level, levelUpStats.Level)
                .WithNewStat(CharacterStat.StatPoints, levelUpStats.StatPoints)
                .WithNewStat(CharacterStat.SkillPoints, levelUpStats.SkillPoints)
                .WithNewStat(CharacterStat.MaxHP, levelUpStats.MaxHp)
                .WithNewStat(CharacterStat.MaxTP, levelUpStats.MaxTp)
                .WithNewStat(CharacterStat.MaxSP, levelUpStats.MaxSp);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
        }

        protected void ShowDroppedItem(NpcKilledData npcKilledData)
        {
            var mapItem = new MapItem(npcKilledData.DropIndex,
                npcKilledData.DropId,
                npcKilledData.DropCoords.X,
                npcKilledData.DropCoords.Y,
                npcKilledData.DropAmount)
                .WithIsNPCDrop(true)
                .WithDropTime(Option.Some(DateTime.Now))
                .WithOwningPlayerID(Option.Some(npcKilledData.KillerId));

            if (_currentMapStateRepository.MapItems.TryGetValue(npcKilledData.DropIndex, out var oldItem))
                _currentMapStateRepository.MapItems.Update(oldItem, mapItem);
            else
                _currentMapStateRepository.MapItems.Add(mapItem);

            foreach (var notifier in _npcActionNotifiers)
                notifier.NPCDropItem(mapItem);
        }

        protected void NotifySpellCast(int playerID)
        {
            foreach (var notifier in _otherCharacterAnimationNotifiers)
                notifier.NotifyTargetNpcSpellCast(playerID);
        }
    }
}
