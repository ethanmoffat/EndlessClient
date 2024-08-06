using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Base class for handling a character sitting down
    /// </summary>
    public abstract class PlayerSitHandlerBase<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
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

        protected void Handle(int playerId, int x, int y, EODirection direction)
        {
            var sitState = Family == PacketFamily.Sit ? SitState.Floor : SitState.Chair;

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
            else if (_currentMapStateRepository.Characters.TryGetValue(playerId, out var oldCharacter))
            {
                var renderProperties = oldCharacter.RenderProperties.WithSitState(sitState)
                    .WithCurrentAction(sitState == SitState.Standing ? CharacterActionState.Standing : CharacterActionState.Sitting)
                    .WithMapX(x)
                    .WithMapY(y)
                    .WithDirection(direction);

                _currentMapStateRepository.Characters.Update(oldCharacter, oldCharacter.WithRenderProperties(renderProperties));
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerId);
            }
        }
    }
}
