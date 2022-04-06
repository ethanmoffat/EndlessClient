using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Interact.Quest
{
    public class QuestDialogData : IQuestDialogData
    {
        public short VendorID { get; private set; }

        public short QuestID { get; private set; }

        public short SessionID { get; private set; }

        public short DialogID { get; private set; }

        public IReadOnlyDictionary<short, string> DialogTitles { get; private set; }

        public IReadOnlyList<string> PageText { get; private set; }

        public IReadOnlyList<(short ActionID, string DisplayText)> Actions { get; private set; }

        public QuestDialogData()
        {
            DialogTitles = new Dictionary<short, string>();
            PageText = new List<string>();
            Actions = new List<(short, string)>();
        }

        private QuestDialogData(short vendorID,
                                short questID,
                                short sessionID,
                                short dialogID,
                                IReadOnlyDictionary<short, string> dialogTitles,
                                IReadOnlyList<string> pageText,
                                IReadOnlyList<(short, string)> actions)
        {
            VendorID = vendorID;
            QuestID = questID;
            SessionID = sessionID;
            DialogID = dialogID;
            DialogTitles = dialogTitles;
            PageText = pageText;
            Actions = actions;
        }

        public IQuestDialogData WithVendorID(short vendorId)
        {
            var copy = MakeCopy(this);
            copy.VendorID = vendorId;
            return copy;
        }

        public IQuestDialogData WithQuestID(short questId)
        {
            var copy = MakeCopy(this);
            copy.QuestID = questId;
            return copy;
        }

        public IQuestDialogData WithSessionID(short sessionId)
        {
            var copy = MakeCopy(this);
            copy.SessionID = sessionId;
            return copy;
        }

        public IQuestDialogData WithDialogID(short dialogId)
        {
            var copy = MakeCopy(this);
            copy.DialogID = dialogId;
            return copy;
        }

        public IQuestDialogData WithDialogTitles(IReadOnlyDictionary<short, string> dialogTitles)
        {
            var copy = MakeCopy(this);
            copy.DialogTitles = dialogTitles;
            return copy;
        }

        public IQuestDialogData WithPageText(IReadOnlyList<string> pageText)
        {
            var copy = MakeCopy(this);
            copy.PageText = pageText;
            return copy;
        }

        public IQuestDialogData WithActions(IReadOnlyList<(short, string)> actions)
        {
            var copy = MakeCopy(this);
            copy.Actions = actions;
            return copy;
        }

        private static QuestDialogData MakeCopy(IQuestDialogData other)
        {
            return new QuestDialogData(
                other.VendorID,
                other.QuestID,
                other.SessionID,
                other.DialogID,
                other.DialogTitles.ToDictionary(k => k.Key, v => v.Value),
                new List<string>(other.PageText),
                new List<(short, string)>(other.Actions));
        }

        public override bool Equals(object obj)
        {
            var other = obj as QuestDialogData;
            if (other == null) return false;

            return other.VendorID == VendorID
                && other.QuestID == QuestID
                && other.SessionID == SessionID
                && other.DialogID == DialogID
                && other.DialogTitles.SequenceEqual(DialogTitles)
                && other.PageText.SequenceEqual(PageText)
                && other.Actions.SequenceEqual(Actions);
        }

        public override int GetHashCode()
        {
            int hashCode = 170256730;
            hashCode = hashCode * -1521134295 + VendorID.GetHashCode();
            hashCode = hashCode * -1521134295 + QuestID.GetHashCode();
            hashCode = hashCode * -1521134295 + SessionID.GetHashCode();
            hashCode = hashCode * -1521134295 + DialogID.GetHashCode();
            hashCode = hashCode * -1521134295 + DialogTitles.Aggregate(170256730, (a, b) => a * -1521134295 + b.GetHashCode());
            hashCode = hashCode * -1521134295 + PageText.Aggregate(170256730, (a, b) => a * -1521134295 + b.GetHashCode());
            hashCode = hashCode * -1521134295 + Actions.Aggregate(170256730, (a, b) => a * -1521134295 + b.GetHashCode());
            return hashCode;
        }
    }

    public interface IQuestDialogData
    {
        /// <summary>
        /// NPC Vendor ID (<see cref="EOLib.IO.Pub.ENFRecord.VendorID" />)
        /// </summary>
        short VendorID { get; }

        /// <summary>
        /// Quest ID, for the current dialog being shown (part of quest state in EO+ parser)
        /// </summary>
        short QuestID { get; }

        /// <summary>
        /// Session ID, not used by eoserv
        /// </summary>
        short SessionID { get; }

        /// <summary>
        /// Dialog ID, not used by eoserv
        /// </summary>
        short DialogID { get; }

        /// <summary>
        /// Quest dialog titles for the current character, keyed on <see cref="VendorID" />.
        /// </summary>
        IReadOnlyDictionary<short, string> DialogTitles { get; }

        /// <summary>
        /// Text for the quest dialog, one entry per page.
        /// </summary>
        IReadOnlyList<string> PageText { get; }

        /// <summary>
        /// Links for the quest dialog, only shown on the last page.
        /// </summary>
        IReadOnlyList<(short ActionID, string DisplayText)> Actions { get; }

        IQuestDialogData WithVendorID(short vendorId);

        IQuestDialogData WithQuestID(short questId);

        IQuestDialogData WithSessionID(short sessionId);

        IQuestDialogData WithDialogID(short dialogId);

        IQuestDialogData WithDialogTitles(IReadOnlyDictionary<short, string> dialogTitles);

        IQuestDialogData WithPageText(IReadOnlyList<string> pageText);

        IQuestDialogData WithActions(IReadOnlyList<(short, string)> actions);
    }
}
