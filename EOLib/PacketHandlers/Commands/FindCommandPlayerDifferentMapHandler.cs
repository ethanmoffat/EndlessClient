using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Localization;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Commands
{
    [AutoMappedType]
    public class FindCommandPlayerDifferentMapHandler : FindCommandHandlerBase<PlayersNet242ServerPacket>
    {
        public override PacketAction Action => PacketAction.Net242;

        public FindCommandPlayerDifferentMapHandler(IChatRepository chatRespository,
                                                    ILocalizedStringFinder localizedStringFinder,
                                                    IPlayerInfoProvider playerInfoProvider)
            : base(chatRespository, localizedStringFinder, playerInfoProvider)
        {
        }

        public override bool HandlePacket(PlayersNet242ServerPacket packet)
        {
            Handle(packet.Name, EOResourceID.STATUS_LABEL_IS_ONLINE_IN_THIS_WORLD);
            return true;
        }
    }
}
