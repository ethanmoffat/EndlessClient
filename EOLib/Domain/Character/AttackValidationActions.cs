// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Extensions;
using EOLib.Domain.Map;

namespace EOLib.Domain.Character
{
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

            var cellState = _mapCellStateProvider.GetCellStateAt(
                _characterProvider.MainCharacter.RenderProperties.GetDestinationX(),
                _characterProvider.MainCharacter.RenderProperties.GetDestinationY());
            if (cellState.NPC.HasValue && cellState.NPC.Value.OpponentID.HasValue &&
                cellState.NPC.Value.OpponentID != _characterProvider.MainCharacter.ID)
                return AttackValidationError.NotYourBattle;

            return AttackValidationError.OK;
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
