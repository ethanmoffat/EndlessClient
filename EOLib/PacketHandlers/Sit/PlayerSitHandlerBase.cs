using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Base class for handling a character sitting down
    /// </summary>
    public abstract class PlayerSitHandlerBase : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketAction Action => PacketAction.Player;

        public PlayerSitHandlerBase(IPlayerInfoProvider playerInfoProvider,
                                    ICharacterRepository characterRepository,
                                    ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerId = packet.ReadShort();
            var x = packet.ReadChar();
            var y = packet.ReadChar();
            var direction = (EODirection)packet.ReadChar();

            var sitState = Family == PacketFamily.Sit ? SitState.Floor : SitState.Chair;

            if (packet.ReadChar() != 0)
                return false;

            if (playerId == _characterRepository.MainCharacter.ID)
            {
                var renderProperties = _characterRepository.MainCharacter.RenderProperties;
                var updatedRenderProperties = renderProperties.WithSitState(sitState)
                    .WithCurrentAction(sitState == SitState.Standing ? CharacterActionState.Standing : CharacterActionState.Sitting)
                    .WithMapX(x)
                    .WithMapY(y)
                    .WithDirection(direction);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(updatedRenderProperties);
            }
            else if (_currentMapStateRepository.Characters.ContainsKey(playerId))
            {
                var oldCharacter = _currentMapStateRepository.Characters[playerId];
                var renderProperties = oldCharacter.RenderProperties.WithSitState(sitState)
                    .WithCurrentAction(sitState == SitState.Standing ? CharacterActionState.Standing : CharacterActionState.Sitting)
                    .WithMapX(x)
                    .WithMapY(y)
                    .WithDirection(direction);

                _currentMapStateRepository.Characters[playerId] = oldCharacter.WithRenderProperties(renderProperties);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerId);
                return true;
            }

            return true;
        }
    }
}
