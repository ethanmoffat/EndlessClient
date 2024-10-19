using System.Linq;

using AutomaticTypeMapper;

using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.Party;
using EOLib.IO;
using EOLib.IO.Repositories;

namespace EOLib.Domain.Spells
{
    [AutoMappedType]
    public class SpellCastValidationActions : ISpellCastValidationActions
    {
        private readonly IPubFileProvider _pubFileProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IPartyDataProvider _partyDataProvider;

        public SpellCastValidationActions(IPubFileProvider pubFileProvider,
                                          ICurrentMapProvider currentMapProvider,
                                          ICharacterProvider characterProvider,
                                          IPartyDataProvider partyDataProvider)
        {
            _pubFileProvider = pubFileProvider;
            _currentMapProvider = currentMapProvider;
            _characterProvider = characterProvider;
            _partyDataProvider = partyDataProvider;
        }

        public SpellCastValidationResult ValidateSpellCast(int spellId)
        {
            if (_characterProvider.MainCharacter.Frozen)
                return SpellCastValidationResult.Frozen;

            var spellData = _pubFileProvider.ESFFile[spellId];

            var stats = _characterProvider.MainCharacter.Stats;
            if (stats[CharacterStat.SP] - spellData.SP < 0)
                return SpellCastValidationResult.ExhaustedNoSp;
            if (stats[CharacterStat.TP] - spellData.TP < 0)
                return SpellCastValidationResult.ExhaustedNoTp;
            if (spellData.Target == SpellTarget.Group && !_partyDataProvider.Members.Any())
                return SpellCastValidationResult.NotMemberOfGroup;

            return SpellCastValidationResult.Ok;
        }

        public SpellCastValidationResult ValidateSpellCast(int spellId, ISpellTargetable spellTarget)
        {
            if (_characterProvider.MainCharacter.Frozen)
                return SpellCastValidationResult.Frozen;

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
            if (_characterProvider.MainCharacter.Frozen)
                return false;

            var weapon = _characterProvider.MainCharacter.RenderProperties.WeaponGraphic;
            return Constants.Instruments.Any(x => x == weapon);
        }
    }

    public interface ISpellCastValidationActions
    {
        SpellCastValidationResult ValidateSpellCast(int spellId);

        SpellCastValidationResult ValidateSpellCast(int spellId, ISpellTargetable spellTarget);

        bool ValidateBard();
    }
}
