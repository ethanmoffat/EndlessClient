using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.AdminInteract
{
    [AutoMappedType]
    public class AdminInteractReply : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var messageType = (AdminMessageType)packet.ReadChar();
            packet.ReadByte();

            var playerName = packet.ReadBreakString();
            var message = packet.ReadBreakString();

            ChatData chatData;
            switch (messageType)
            {
                case AdminMessageType.Message:
                    chatData = new ChatData(ChatTab.Group, playerName, $"needs help: {message}", ChatIcon.Information, ChatColor.ServerGlobal, filter: false);
                    break;
                case AdminMessageType.Report:
                    var reporteeName = packet.ReadBreakString();
                    chatData = new ChatData(ChatTab.Group, playerName, $"reports: {reporteeName}, {message}", ChatIcon.Information, ChatColor.ServerGlobal, filter: false);
                    break;
                default:
                    return false;
            }

            _chatRepository.AllChat[ChatTab.Group].Add(chatData);

            return true;
        }
    }
}
