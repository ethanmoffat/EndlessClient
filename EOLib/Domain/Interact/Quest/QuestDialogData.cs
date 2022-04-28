using Amadevus.RecordGenerator;
using System.Collections.Generic;

namespace EOLib.Domain.Interact.Quest
{
    [Record]
    public sealed partial class QuestDialogData
    {
        public short VendorID { get; }

        public short QuestID { get; }

        public short SessionID { get; }

        public short DialogID { get; }

        public IReadOnlyDictionary<short, string> DialogTitles { get; }

        public IReadOnlyList<string> PageText { get; }

        public IReadOnlyList<(short ActionID, string DisplayText)> Actions { get; }

        public QuestDialogData()
        {
            DialogTitles = new Dictionary<short, string>();
            PageText = new List<string>();
            Actions = new List<(short, string)>();
        }
    }
}
