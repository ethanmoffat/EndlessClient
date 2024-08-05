using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Handle another player standing up
    /// </summary>
    [AutoMappedType]
    public class SitRemoveHandler : PlayerStandHandlerBase<SitRemoveServerPacket>
    {
        public override PacketAction Action => PacketAction.Remove;

        public SitRemoveHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }

        public override bool HandlePacket(SitRemoveServerPacket packet)
        {
            Handle(packet.PlayerId, packet.Coords.X, packet.Coords.Y);
            return true;
        }
    }
}