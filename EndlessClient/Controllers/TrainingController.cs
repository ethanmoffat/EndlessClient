using AutomaticTypeMapper;
using EOLib.Domain.Character;

namespace EndlessClient.Controllers
{
    [MappedType(BaseType = typeof(ITrainingController))]
    public class TrainingController : ITrainingController
    {
        private readonly ITrainingActions _trainingActions;

        public TrainingController(ITrainingActions trainingActions)
        {
            _trainingActions = trainingActions;
        }

        public void AddStatPoint(CharacterStat whichStat)
        {
            if (InvalidStat(whichStat))
                return;

            _trainingActions.LevelUpStat(whichStat);
        }

        public void AddSkillPoint(int spellId)
        {
            _trainingActions.LevelUpSkill(spellId);
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

        void AddSkillPoint(int spellId);
    }
}
