using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Sent when a player sits on the floor via F11
    /// </summary>
    [AutoMappedType]
    public class SitPlayerHandler : PlayerSitHandlerBase<SitPlayerServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Sit;

        public SitPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }

        public override bool HandlePacket(SitPlayerServerPacket packet)
        {
            Handle(packet.PlayerId, packet.Coords.X, packet.Coords.Y, (EODirection)packet.Direction);
            return true;
        }
    }
}
