using System;
using System.Collections.Generic;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public enum DialogReply : byte
    {
        Ok = 1,
        Link
    }

    public enum QuestPage : byte
    {
        Progress = 1,
        History
    }

    public enum BookIcon : byte
    {
        Item = 3,
        Talk = 5,
        Kill = 8,
        Step = 10
    }

    /// <summary>
    /// State object for quest transactions with server
    /// </summary>
    public struct QuestState
    {
        private readonly short _npcIndex;
        private readonly short _questID;
        private readonly short _dialogID;
        private readonly short _sessionID;

        public short SessionID => _sessionID;
        public short DialogID => _dialogID;
        public short QuestID => _questID;
        public short NPCIndex => _npcIndex;
        public short VendorID => _npcIndex;
//re-used

        public QuestState(short session, short dialogID, short questID, short npcIndex)
        {
            _sessionID = session;
            _dialogID = dialogID;
            _questID = questID;
            _npcIndex = npcIndex;
        }
    }

    /// <summary>
    /// Data regarding in-progress quests
    /// </summary>
    public struct InProgressQuestData
    {
        private readonly string _name;
        private readonly string _description;
        private readonly BookIcon _icon;
        private readonly short _progress;
        private readonly short _target;

        public string Name => _name;
        public string Description => _description;
        public BookIcon Icon => _icon;

        public int IconIndex
        {
            get
            {
                //these are probably wrong. can't really tell what it's supposed to be from original
                switch (Icon)
                {
                    case BookIcon.Item:
                        return 2;
                    case BookIcon.Talk:
                        return 1;
                    case BookIcon.Kill:
                        return 3;
                    case BookIcon.Step:
                        return 4;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public short Progress => _progress;

        public short Target => _target;

        internal InProgressQuestData(OldPacket pkt)
        {
            _name = pkt.GetBreakString();
            _description = pkt.GetBreakString();
            _icon = (BookIcon) pkt.GetShort();
            _progress = pkt.GetShort();
            _target = pkt.GetShort();
            if (pkt.GetByte() != 255)
                throw new ArgumentException("Malformed quest packet", nameof(pkt));
        }
    }

    public delegate void QuestDialogEvent(QuestState stateInfo, Dictionary<short, string> dialogNames, List<string> pages, Dictionary<short, string> links);

    public delegate void ViewQuestProgressEvent(short numQuests, List<InProgressQuestData> questInfo);

    public delegate void ViewQuestHistoryEvent(short numQuests, List<string> completedQuestNames);

    partial class PacketAPI
    {
        public event ViewQuestProgressEvent OnViewQuestProgress;
        public event ViewQuestHistoryEvent OnViewQuestHistory;

        private void _createQuestMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Quest, PacketAction.List), _handleQuestList, true);
        }

        public bool RequestQuestHistory(QuestPage page)
        {
            if (!Initialized || !m_client.ConnectedAndInitialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Quest, PacketAction.List);
            pkt.AddChar((byte) page);

            return m_client.SendPacket(pkt);
        }

        private void _handleQuestList(OldPacket pkt)
        {
            QuestPage page = (QuestPage) pkt.GetChar();
            short numQuests = pkt.GetShort();

            switch (page)
            {
                case QuestPage.Progress:
                    var dataCollection = new List<InProgressQuestData>(numQuests);
                    while (pkt.ReadPos != pkt.Length)
                        dataCollection.Add(new InProgressQuestData(pkt));

                    if (OnViewQuestProgress != null)
                        OnViewQuestProgress(numQuests, dataCollection);

                    break;
                case QuestPage.History:
                    var completedNames = new List<string>(numQuests);
                    while (pkt.ReadPos != pkt.Length)
                        completedNames.Add(pkt.GetBreakString());

                    if (OnViewQuestHistory != null)
                        OnViewQuestHistory(numQuests, completedNames);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
