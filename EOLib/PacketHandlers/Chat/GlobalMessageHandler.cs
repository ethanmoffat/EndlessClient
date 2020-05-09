using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Net;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class GlobalMessageHandler : PlayerChatByNameBase
    {
        private readonly IChatRepository _chatRepository;

        public override PacketAction Action => PacketAction.Message;

        public GlobalMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                    IChatRepository chatRepository)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
        }

        protected override void PostChat(string name, string message)
        {
            var data = new ChatData(name, message, ChatIcon.GlobalAnnounce);
            _chatRepository.AllChat[ChatTab.Global].Add(data);
        }
    }
}