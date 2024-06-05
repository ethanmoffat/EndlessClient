using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class GlobalMessageHandler : PlayerChatByNameBase<TalkMsgServerPacket>
    {
        private readonly IChatRepository _chatRepository;

        public override PacketAction Action => PacketAction.Msg;

        public GlobalMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                    IChatRepository chatRepository)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
        }

        public override bool HandlePacket(TalkMsgServerPacket packet)
        {
            return Handle(packet.PlayerName, packet.Message);
        }

        protected override void PostChat(string name, string message)
        {
            var data = new ChatData(ChatTab.Global, name, message, ChatIcon.GlobalAnnounce);
            _chatRepository.AllChat[ChatTab.Global].Add(data);
        }
    }
}