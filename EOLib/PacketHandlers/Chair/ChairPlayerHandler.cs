using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.PacketHandlers.Sit;

namespace EOLib.PacketHandlers.Chair
{
    /// <summary>
    /// Handle a player sitting in a chair
    /// </summary>
    [AutoMappedType]
    public class ChairPlayerHandler : PlayerSitHandlerBase
    {
        public override PacketFamily Family => PacketFamily.Chair;

        public ChairPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICharacterRepository characterRepository,
                                  ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }
    }
}
