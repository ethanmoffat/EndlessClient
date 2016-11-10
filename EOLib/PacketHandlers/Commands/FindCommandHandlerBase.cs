// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Commands
{
    public abstract class FindCommandHandlerBase : InGameOnlyPacketHandler
    {
        private readonly IChatRepository _chatRespository;
        private readonly ILocalizedStringService _localizedStringService;

        public override PacketFamily Family { get { return PacketFamily.Players; } }

        protected abstract EOResourceID ResourceIDForResponse { get; }

        protected FindCommandHandlerBase(IChatRepository chatRespository,
                                         ILocalizedStringService localizedStringService,
                                         IPlayerInfoProvider playerInfoProvider)
            : base(playerInfoProvider)
        {
            _chatRespository = chatRespository;
            _localizedStringService = localizedStringService;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerName = packet.ReadEndString();
            var message = string.Format("{0} {1}",
                char.ToUpper(playerName[0]) + playerName.Substring(1),
                _localizedStringService.GetString(ResourceIDForResponse));

            var chatData = new ChatData("System", message, ChatIcon.LookingDude);
            _chatRespository.AllChat[ChatTab.Local].Add(chatData);

            return true;
        }
    }
}
