using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Net;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class GuildMessageHandler : PlayerChatByNameBase
    {
        private readonly IChatRepository _chatRepository;

        public override PacketAction Action => PacketAction.Request;

        public GuildMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                   IChatRepository chatRepository)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
        }

        protected override void PostChat(string name, string message)
        {
            var data = new ChatData(ChatTab.Group, name, message);
            _chatRepository.AllChat[ChatTab.Group].Add(data);
        }
    }
}