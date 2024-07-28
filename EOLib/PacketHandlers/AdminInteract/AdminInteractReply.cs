using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.AdminInteract
{
    /// <summary>
    /// Received by admins when a report is made by another player.
    /// </summary>
    [AutoMappedType]
    public class AdminInteractReply : InGameOnlyPacketHandler<AdminInteractReplyServerPacket>
    {
        private readonly IChatRepository _chatRepository;

        public override PacketFamily Family => PacketFamily.AdminInteract;

        public override PacketAction Action => PacketAction.Reply;

        public AdminInteractReply(IPlayerInfoProvider playerInfoProvider,
                                  IChatRepository chatRepository)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
        }

        public override bool HandlePacket(AdminInteractReplyServerPacket packet)
        {
            ChatData chatData;
            switch (packet.MessageType)
            {
                case AdminMessageType.Message:
                    {
                        var message = (AdminInteractReplyServerPacket.MessageTypeDataMessage)packet.MessageTypeData;
                        chatData = new ChatData(ChatTab.Group, message.PlayerName, $"needs help: {message}", ChatIcon.Information, ChatColor.ServerGlobal, filter: false);
                    }
                    break;
                case AdminMessageType.Report:
                    {
                        var report = (AdminInteractReplyServerPacket.MessageTypeDataReport)packet.MessageTypeData;
                        chatData = new ChatData(ChatTab.Group, report.PlayerName, $"reports: {report.ReporteeName}, {report.Message}", ChatIcon.Information, ChatColor.ServerGlobal, filter: false);
                    }
                    break;
                default:
                    return false;
            }

            _chatRepository.AllChat[ChatTab.Group].Add(chatData);

            return true;
        }
    }
}