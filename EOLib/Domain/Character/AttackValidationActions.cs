using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Repositories;

namespace EOLib.Domain.Character
{
    [AutoMappedType]
    public class AttackValidationActions : IAttackValidationActions
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IENFFileProvider _enfFileProvider;

        public AttackValidationActions(ICharacterProvider characterProvider,
                                       IMapCellStateProvider mapCellStateProvider,
                                       IEIFFileProvider eifFileProvider,
                                       IENFFileProvider enfFileProvider)
        {
            _characterProvider = characterProvider;
            _mapCellStateProvider = mapCellStateProvider;
            _eifFileProvider = eifFileProvider;
            _enfFileProvider = enfFileProvider;
        }

        public AttackValidationError ValidateCharacterStateBeforeAttacking()
        {
            if (_characterProvider.MainCharacter.Frozen)
                return AttackValidationError.Frozen;

            if (_characterProvider.MainCharacter.Stats[CharacterStat.Weight] >
                _characterProvider.MainCharacter.Stats[CharacterStat.MaxWeight])
                return AttackValidationError.Overweight;

            if (_characterProvider.MainCharacter.Stats[CharacterStat.SP] <= 0)
                return AttackValidationError.Exhausted;

            var rp = _characterProvider.MainCharacter.RenderProperties;

            var isRangedWeapon = _eifFileProvider.EIFFile
                .Where(x => x.Type == IO.ItemType.Weapon && x.SubType == IO.ItemSubType.Ranged)
                .Any(x => x.DollGraphic == rp.WeaponGraphic);
            var isArrows = _eifFileProvider.EIFFile
                .Where(x => x.Type == IO.ItemType.Shield && x.SubType == IO.ItemSubType.Arrows)
                .Any(x => x.DollGraphic == rp.ShieldGraphic);

            if (isRangedWeapon && (rp.ShieldGraphic == 0 || !isArrows))
                return AttackValidationError.MissingArrows;

            return _mapCellStateProvider
                .GetCellStateAt(rp.GetDestinationX(), rp.GetDestinationY())
                .NPC.Match(
                    some: npc => npc.OpponentID.Match(
                        some: id =>
                        {
                            var notYourBattle = id != _characterProvider.MainCharacter.ID;
                            var isBossNpc = _enfFileProvider.ENFFile[npc.ID].Boss > 0;
                            return notYourBattle && !isBossNpc
                                ? AttackValidationError.NotYourBattle
                                : AttackValidationError.OK;
                        },
                        none: () => AttackValidationError.OK),
                    none: () => AttackValidationError.OK);
        }
    }

    public interface IAttackValidationActions
    {
        AttackValidationError ValidateCharacterStateBeforeAttacking();
    }

    public enum AttackValidationError
    {
        OK,
        Overweight,
        Exhausted,
        NotYourBattle,
        MissingArrows,
        Frozen,
    }
}
