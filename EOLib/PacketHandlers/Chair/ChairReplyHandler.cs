using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.PacketHandlers.Sit;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chair
{
    /// <summary>
    /// Handle your player sitting in a chair
    /// </summary>
    [AutoMappedType]
    public class ChairReplyHandler : PlayerSitHandlerBase<ChairReplyServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Chair;

        public override PacketAction Action => PacketAction.Reply;

        public ChairReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICharacterRepository characterRepository,
                                  ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }

        public override bool HandlePacket(ChairReplyServerPacket packet)
        {
            Handle(packet.PlayerId, packet.Coords.X, packet.Coords.Y, (EODirection)packet.Direction);
            return true;
        }
    }
}
