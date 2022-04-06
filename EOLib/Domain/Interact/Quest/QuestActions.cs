using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

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

        public void RespondToQuestDialog(DialogReply reply, byte linkId = 0)
        {
            _questDataProvider.QuestDialogData.MatchSome(data =>
            {
                var builder = new PacketBuilder(PacketFamily.Quest, PacketAction.Accept)
                    .AddShort(data.SessionID) // ignored by eoserv
                    .AddShort(data.DialogID) // ignored by eoserv
                    .AddShort(data.QuestID)
                    .AddShort(data.VendorID) // ignored by eoserv
                    .AddChar((byte)reply);

                if (reply == DialogReply.Link)
                    builder = builder.AddChar(linkId);

                var packet = builder.Build();
                _packetSendService.SendPacket(packet);
            });
        }
    }

    public interface IQuestActions
    {
        void RespondToQuestDialog(DialogReply reply, byte linkId = 0);
    }
}
