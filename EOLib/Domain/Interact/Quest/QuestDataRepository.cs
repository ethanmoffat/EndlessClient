using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.NPC;
using Optional;

namespace EOLib.Domain.Interact.Quest
{
    public interface IQuestDataRepository : IResettable
    {
        NPC.NPC RequestedNPC { get; set; }

        Option<QuestDialogData> QuestDialogData { get; set; }

        List<QuestProgressData> QuestProgress { get; set; }

        List<string> QuestHistory { get; set; }
    }

    public interface IQuestDataProvider : IResettable
    {
        NPC.NPC RequestedNPC { get; }

        Option<QuestDialogData> QuestDialogData { get; }

        IReadOnlyList<QuestProgressData> QuestProgress { get; }

        IReadOnlyList<string> QuestHistory { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class QuestDataRepository : IQuestDataProvider, IQuestDataRepository
    {
        public NPC.NPC RequestedNPC { get; set; }

        public Option<QuestDialogData> QuestDialogData { get; set; }

        public List<QuestProgressData> QuestProgress { get; set; }

        public List<string> QuestHistory { get; set; }

        IReadOnlyList<QuestProgressData> IQuestDataProvider.QuestProgress => QuestProgress;

        IReadOnlyList<string> IQuestDataProvider.QuestHistory => QuestHistory;

        public QuestDataRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            QuestDialogData = Option.None<QuestDialogData>();
            QuestProgress = new List<QuestProgressData>();
            QuestHistory = new List<string>();
        }
    }
}
