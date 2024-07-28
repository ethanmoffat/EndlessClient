using Amadevus.RecordGenerator;
using System.Collections.Generic;

namespace EOLib.Domain.Interact.Quest
{
    [Record]
    public sealed partial class QuestDialogData
    {
        public int VendorID { get; }

        public int QuestID { get; }

        public int SessionID { get; }

        public int DialogID { get; }

        public IReadOnlyDictionary<int, string> DialogTitles { get; }

        public IReadOnlyList<string> PageText { get; }

        public IReadOnlyList<(int ActionID, string DisplayText)> Actions { get; }

        public QuestDialogData()
        {
            DialogTitles = new Dictionary<int, string>();
            PageText = new List<string>();
            Actions = new List<(int, string)>();
        }
    }
}