using AutomaticTypeMapper;
using Optional;

namespace EOLib.Domain.Interact.Quest
{
    public interface IQuestDataRepository : IResettable
    {
        Option<IQuestDialogData> QuestDialogData { get; set; }
    }

    public interface IQuestDataProvider : IResettable
    {
        Option<IQuestDialogData> QuestDialogData { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class QuestDataRepository : IQuestDataProvider, IQuestDataRepository
    {
        public Option<IQuestDialogData> QuestDialogData { get; set; }

        public QuestDataRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            QuestDialogData = Option.None<IQuestDialogData>();
        }
    }
}
