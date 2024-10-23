using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class GuildMessageHandler : PlayerChatByNameBase<TalkRequestServerPacket>
    {
        private readonly IChatRepository _chatRepository;

        public override PacketAction Action => PacketAction.Request;

        public GuildMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                   IChatRepository chatRepository)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
        }

        public override bool HandlePacket(TalkRequestServerPacket packet)
        {
            return Handle(packet.PlayerName, packet.Message);
        }

        protected override void PostChat(string name, string message)
        {
            var data = new ChatData(ChatTab.Group, name, message, ChatIcon.Guild, ChatColor.PM);
            _chatRepository.AllChat[ChatTab.Group].Add(data);
        }
    }
}
