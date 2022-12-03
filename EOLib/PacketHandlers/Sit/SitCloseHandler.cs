using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Handle the main player standing up
    /// </summary>
    [AutoMappedType]
    public class SitCloseHandler : PlayerStandHandlerBase
    {
        public override PacketAction Action => PacketAction.Close;

        public SitCloseHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }
    }
}
