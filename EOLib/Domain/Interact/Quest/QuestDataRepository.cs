using AutomaticTypeMapper;
using EOLib.Domain.NPC;
using Optional;

namespace EOLib.Domain.Interact.Quest
{
    public interface IQuestDataRepository : IResettable
    {
        INPC RequestedNPC { get; set; }

        Option<IQuestDialogData> QuestDialogData { get; set; }
    }

    public interface IQuestDataProvider : IResettable
    {
        INPC RequestedNPC { get; }

        Option<IQuestDialogData> QuestDialogData { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class QuestDataRepository : IQuestDataProvider, IQuestDataRepository
    {
        public INPC RequestedNPC { get; set; }

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
