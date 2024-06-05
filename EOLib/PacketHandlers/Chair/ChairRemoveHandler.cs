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
    /// Handle another player standing up from a chair
    /// </summary>
    [AutoMappedType]
    public class ChairRemoveHandler : PlayerStandHandlerBase<ChairRemoveServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Chair;

        public override PacketAction Action => PacketAction.Remove;

        public ChairRemoveHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }

        public override bool HandlePacket(ChairRemoveServerPacket packet)
        {
            Handle(packet.PlayerId, packet.Coords.X, packet.Coords.Y);
            return true;
        }
    }
}
