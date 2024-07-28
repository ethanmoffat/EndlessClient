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
    /// Handle a player sitting in a chair
    /// </summary>
    [AutoMappedType]
    public class ChairPlayerHandler : PlayerSitHandlerBase<ChairPlayerServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Chair;

        public ChairPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICharacterRepository characterRepository,
                                  ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }

        public override bool HandlePacket(ChairPlayerServerPacket packet)
        {
            Handle(packet.PlayerId, packet.Coords.X, packet.Coords.Y, (EODirection)packet.Direction);
            return true;
        }
    }
}