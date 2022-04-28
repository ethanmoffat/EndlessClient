using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.IO;
using EOLib.IO.Repositories;
using Optional.Collections;
using System.Linq;

namespace EOLib.Domain.Spells
{
    [AutoMappedType]
    public class SpellCastValidationActions : ISpellCastValidationActions
    {
        private readonly IPubFileProvider _pubFileProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICharacterProvider _characterProvider;

        public SpellCastValidationActions(IPubFileProvider pubFileProvider,
                                          ICurrentMapProvider currentMapProvider,
                                          ICharacterProvider characterProvider)
        {
            _pubFileProvider = pubFileProvider;
            _currentMapProvider = currentMapProvider;
            _characterProvider = characterProvider;
        }

        public SpellCastValidationResult ValidateSpellCast(int spellId)
        {
            var spellData = _pubFileProvider.ESFFile[spellId];

            var stats = _characterProvider.MainCharacter.Stats;
            if (stats[CharacterStat.SP] - spellData.SP < 0)
                return SpellCastValidationResult.ExhaustedNoSp;
            if (stats[CharacterStat.TP] - spellData.TP < 0)
                return SpellCastValidationResult.ExhaustedNoTp;

            return SpellCastValidationResult.Ok;
        }

        public SpellCastValidationResult ValidateSpellCast(int spellId, ISpellTargetable spellTarget)
        {
            var res = ValidateSpellCast(spellId);
            if (res != SpellCastValidationResult.Ok)
                return res;

            var spellData = _pubFileProvider.ESFFile[spellId];

            if (spellTarget is NPC.NPC)
            {
                if (spellData.TargetRestrict == SpellTargetRestrict.Friendly ||
                    spellData.Target != SpellTarget.Normal ||
                    spellData.Type != SpellType.Damage)
                    return SpellCastValidationResult.WrongTargetType;

                var npcData = _pubFileProvider.ENFFile[spellTarget.ID];

                if (npcData.Type != NPCType.Passive && npcData.Type != NPCType.Aggressive)
                    return SpellCastValidationResult.CannotAttackNPC;
            }
            else if (spellTarget is Character.Character)
            {
                if (spellData.TargetRestrict == SpellTargetRestrict.NPCOnly ||
                    spellData.Target != SpellTarget.Normal ||
                    (spellTarget == _characterProvider.MainCharacter && spellData.TargetRestrict != SpellTargetRestrict.Friendly) ||
                    (spellData.Type != SpellType.Heal && spellData.Type != SpellType.Damage) ||
                    (!_currentMapProvider.CurrentMap.Properties.PKAvailable && spellData.TargetRestrict == SpellTargetRestrict.Opponent))
                    return SpellCastValidationResult.WrongTargetType;
            }
            else
            {
                return SpellCastValidationResult.WrongTargetType;
            }

            return SpellCastValidationResult.Ok;
        }

        public bool ValidateBard()
        {
            var weapon = _characterProvider.MainCharacter.RenderProperties.WeaponGraphic;
            return _pubFileProvider.EIFFile.SingleOrNone(x => x.DollGraphic == weapon && x.Type == ItemType.Weapon)
                .Match(some => Constants.InstrumentIDs.Any(x => x == some.ID), () => false);
        }
    }

    public interface ISpellCastValidationActions
    {
        SpellCastValidationResult ValidateSpellCast(int spellId);

        SpellCastValidationResult ValidateSpellCast(int spellId, ISpellTargetable spellTarget);

        bool ValidateBard();
    }
}
