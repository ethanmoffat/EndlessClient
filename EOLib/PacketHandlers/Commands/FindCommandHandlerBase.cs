using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Localization;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.PacketHandlers.Commands
{
    public abstract class FindCommandHandlerBase<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
    {
        private readonly IChatRepository _chatRespository;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public override PacketFamily Family => PacketFamily.Players;

        protected FindCommandHandlerBase(IChatRepository chatRespository,
                                         ILocalizedStringFinder localizedStringFinder,
                                         IPlayerInfoProvider playerInfoProvider)
            : base(playerInfoProvider)
        {
            _chatRespository = chatRespository;
            _localizedStringFinder = localizedStringFinder;
        }

        protected void Handle(string playerName, EOResourceID resourceId)
        {
            var message = $"{char.ToUpper(playerName[0]) + playerName.Substring(1)} {_localizedStringFinder.GetString(resourceId)}";
            var chatData = new ChatData(ChatTab.Local, "System", message, ChatIcon.LookingDude);
            _chatRespository.AllChat[ChatTab.Local].Add(chatData);
        }
    }
}
