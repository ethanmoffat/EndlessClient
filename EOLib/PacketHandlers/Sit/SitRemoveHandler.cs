using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Handle another player standing up
    /// </summary>
    [AutoMappedType]
    public class SitRemoveHandler : PlayerStandHandlerBase
    {
        public override PacketAction Action => PacketAction.Remove;

        public SitRemoveHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }
    }
}
