// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;
using EOLib.Domain.Character;

namespace EndlessClient.Controllers
{
    [MappedType(BaseType = typeof(ITrainingController))]
    public class TrainingController : ITrainingController
    {
        private readonly IStatTrainingActions _statTrainingActions;

        public TrainingController(IStatTrainingActions statTrainingActions)
        {
            _statTrainingActions = statTrainingActions;
        }

        public void AddStatPoint(CharacterStat whichStat)
        {
            if (InvalidStat(whichStat))
                return;

            _statTrainingActions.LevelUpStat(whichStat);
        }

        private static bool InvalidStat(CharacterStat whichStat)
        {
            switch (whichStat)
            {
                case CharacterStat.Strength:
                case CharacterStat.Intelligence:
                case CharacterStat.Wisdom:
                case CharacterStat.Agility:
                case CharacterStat.Constituion:
                case CharacterStat.Charisma: return false;
                default: return true;
            }
        }
    }

    public interface ITrainingController
    {
        void AddStatPoint(CharacterStat whichStat);
    }
}
