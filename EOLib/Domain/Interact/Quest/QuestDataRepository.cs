using AutomaticTypeMapper;
using EOLib.Domain.NPC;
using Optional;
using System.Collections.Generic;

namespace EOLib.Domain.Interact.Quest
{
    public interface IQuestDataRepository : IResettable
    {
        INPC RequestedNPC { get; set; }

        Option<IQuestDialogData> QuestDialogData { get; set; }

        List<IQuestProgressData> QuestProgress { get; set; }

        List<string> QuestHistory { get; set; }
    }

    public interface IQuestDataProvider : IResettable
    {
        INPC RequestedNPC { get; }

        Option<IQuestDialogData> QuestDialogData { get; }

        IReadOnlyList<IQuestProgressData> QuestProgress { get; }

        IReadOnlyList<string> QuestHistory { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class QuestDataRepository : IQuestDataProvider, IQuestDataRepository
    {
        public INPC RequestedNPC { get; set; }

        public Option<IQuestDialogData> QuestDialogData { get; set; }

        public List<IQuestProgressData> QuestProgress { get; set; }

        public List<string> QuestHistory { get; set; }

        IReadOnlyList<IQuestProgressData> IQuestDataProvider.QuestProgress => QuestProgress;

        IReadOnlyList<string> IQuestDataProvider.QuestHistory => QuestHistory;

        public QuestDataRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            QuestDialogData = Option.None<IQuestDialogData>();
            QuestProgress = new List<IQuestProgressData>();
            QuestHistory = new List<string>();
        }
    }
}
