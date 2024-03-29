﻿using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Commands
{
    public abstract class FindCommandHandlerBase : InGameOnlyPacketHandler
    {
        private readonly IChatRepository _chatRespository;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public override PacketFamily Family => PacketFamily.Players;

        protected abstract EOResourceID ResourceIDForResponse { get; }

        protected FindCommandHandlerBase(IChatRepository chatRespository,
                                         ILocalizedStringFinder localizedStringFinder,
                                         IPlayerInfoProvider playerInfoProvider)
            : base(playerInfoProvider)
        {
            _chatRespository = chatRespository;
            _localizedStringFinder = localizedStringFinder;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerName = packet.ReadEndString();
            var message =
                $"{char.ToUpper(playerName[0]) + playerName.Substring(1)} {_localizedStringFinder.GetString(ResourceIDForResponse)}";

            var chatData = new ChatData(ChatTab.Local, "System", message, ChatIcon.LookingDude);
            _chatRespository.AllChat[ChatTab.Local].Add(chatData);

            return true;
        }
    }
}
