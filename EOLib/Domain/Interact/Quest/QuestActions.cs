using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Interact.Quest
{
    [AutoMappedType]
    public class QuestActions : IQuestActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IQuestDataProvider _questDataProvider;

        public QuestActions(IPacketSendService packetSendService,
                            IQuestDataProvider questDataProvider)
        {
            _packetSendService = packetSendService;
            _questDataProvider = questDataProvider;
        }

        public void RequestQuest(int npcIndex, int questId) =>
            _packetSendService.SendPacket(new QuestUseClientPacket { NpcIndex = npcIndex, QuestId = questId });

        public void RespondToQuestDialog(DialogReply reply, int linkId = 0)
        {
            _questDataProvider.QuestDialogData.MatchSome(data =>
            {
                var packet = new QuestAcceptClientPacket
                {
                    SessionId = data.SessionID,
                    DialogId = data.DialogID,
                    QuestId = data.QuestID,
                    NpcIndex = data.VendorID,
                    ReplyType = reply,
                    ReplyTypeData = reply == DialogReply.Link
                        ? new QuestAcceptClientPacket.ReplyTypeDataLink { Action = linkId }
                        : null
                };
                _packetSendService.SendPacket(packet);
            });
        }

        public void RequestQuestHistory(QuestPage page)
        {
            var packet = new QuestListClientPacket
            {
                Page = page
            };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IQuestActions
    {
        void RequestQuest(int npcIndex, int questId);

        void RespondToQuestDialog(DialogReply reply, int linkId = 0);

        void RequestQuestHistory(QuestPage page);
    }
}
