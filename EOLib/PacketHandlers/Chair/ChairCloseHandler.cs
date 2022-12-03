using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.PacketHandlers.Sit;

namespace EOLib.PacketHandlers.Chair
{
    /// <summary>
    /// Handle the main player standing up from a chair
    /// </summary>
    [AutoMappedType]
    public class ChairCloseHandler : SitCloseHandler
    {
        public override PacketFamily Family => PacketFamily.Chair;

        public ChairCloseHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterRepository characterRepository,
                                 ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }
    }
}
