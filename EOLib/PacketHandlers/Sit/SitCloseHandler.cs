using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Handle the main player standing up
    /// </summary>
    [AutoMappedType]
    public class SitCloseHandler : PlayerStandHandlerBase<SitCloseServerPacket>
    {
        public override PacketAction Action => PacketAction.Close;

        public SitCloseHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }

        public override bool HandlePacket(SitCloseServerPacket packet)
        {
            Handle(packet.PlayerId, packet.Coords.X, packet.Coords.Y);
            return true;
        }
    }
}
