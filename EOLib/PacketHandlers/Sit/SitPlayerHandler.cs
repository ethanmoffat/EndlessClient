using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Sent when a player sits on the floor via F11
    /// </summary>
    [AutoMappedType]
    public class SitPlayerHandler : PlayerSitHandlerBase
    {
        public override PacketFamily Family => PacketFamily.Sit;

        public SitPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }
    }
}
