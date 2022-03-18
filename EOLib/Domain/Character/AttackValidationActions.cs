using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;

namespace EOLib.Domain.Character
{
    [AutoMappedType]
    public class AttackValidationActions : IAttackValidationActions
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly IMapCellStateProvider _mapCellStateProvider;

        public AttackValidationActions(ICharacterProvider characterProvider,
                                       IMapCellStateProvider mapCellStateProvider)
        {
            _characterProvider = characterProvider;
            _mapCellStateProvider = mapCellStateProvider;
        }

        public AttackValidationError ValidateCharacterStateBeforeAttacking()
        {
            if (_characterProvider.MainCharacter.Stats[CharacterStat.Weight] >
                _characterProvider.MainCharacter.Stats[CharacterStat.MaxWeight])
                return AttackValidationError.Overweight;
            if (_characterProvider.MainCharacter.Stats[CharacterStat.SP] <= 0)
                return AttackValidationError.Exhausted;

            var rp = _characterProvider.MainCharacter.RenderProperties;

            return _mapCellStateProvider
                .GetCellStateAt(rp.GetDestinationX(), rp.GetDestinationY())
                .NPC.Match(
                    some: npc => npc.OpponentID.Map(id => id != _characterProvider.MainCharacter.ID ? AttackValidationError.NotYourBattle : AttackValidationError.OK).ValueOr(AttackValidationError.OK),
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
        NotYourBattle
    }
}
