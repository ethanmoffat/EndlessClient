using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Localization;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Commands
{
    [AutoMappedType]
    public class FindCommandPlayerSameMapHandler : FindCommandHandlerBase<PlayersPongServerPacket>
    {
        public override PacketAction Action => PacketAction.Pong;

        public FindCommandPlayerSameMapHandler(IChatRepository chatRespository,
                                               ILocalizedStringFinder localizedStringFinder,
                                               IPlayerInfoProvider playerInfoProvider)
            : base(chatRespository, localizedStringFinder, playerInfoProvider)
        {
        }

        public override bool HandlePacket(PlayersPongServerPacket packet)
        {
            Handle(packet.Name, EOResourceID.STATUS_LABEL_IS_ONLINE_SAME_MAP);
            return true;
        }
    }
}