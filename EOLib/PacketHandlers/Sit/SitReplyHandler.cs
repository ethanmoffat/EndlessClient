using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Handle your player sitting
    /// </summary>
    [AutoMappedType]
    public class SitReplyHandler : PlayerSitHandlerBase<SitReplyServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Sit;

        public override PacketAction Action => PacketAction.Reply;

        public SitReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICharacterRepository characterRepository,
                                  ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }

        public override bool HandlePacket(SitReplyServerPacket packet)
        {
            Handle(packet.PlayerId, packet.Coords.X, packet.Coords.Y, (EODirection)packet.Direction);
            return true;
        }
    }
}