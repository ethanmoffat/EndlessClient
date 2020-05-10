using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Localization;
using EOLib.Net;

namespace EOLib.PacketHandlers.Commands
{
    [AutoMappedType]
    public class FindCommandPlayerSameMapHandler : FindCommandHandlerBase
    {
        public override PacketAction Action => PacketAction.Pong;

        protected override EOResourceID ResourceIDForResponse => EOResourceID.STATUS_LABEL_IS_ONLINE_SAME_MAP;

        public FindCommandPlayerSameMapHandler(IChatRepository chatRespository,
                                               ILocalizedStringFinder localizedStringFinder,
                                               IPlayerInfoProvider playerInfoProvider)
            : base(chatRespository, localizedStringFinder, playerInfoProvider)
        {
        }
    }
}