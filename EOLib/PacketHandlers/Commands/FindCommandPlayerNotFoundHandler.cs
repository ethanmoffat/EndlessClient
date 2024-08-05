using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Localization;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Commands
{
    [AutoMappedType]
    public class FindCommandPlayerNotFoundHandler : FindCommandHandlerBase<PlayersPingServerPacket>
    {
        public override PacketAction Action => PacketAction.Ping;

        public FindCommandPlayerNotFoundHandler(IChatRepository chatRespository,
                                                ILocalizedStringFinder localizedStringFinder,
                                                IPlayerInfoProvider playerInfoProvider)
            : base(chatRespository, localizedStringFinder, playerInfoProvider)
        {
        }

        public override bool HandlePacket(PlayersPingServerPacket packet)
        {
            Handle(packet.Name, EOResourceID.STATUS_LABEL_IS_ONLINE_NOT_FOUND);
            return true;
        }
    }
}