using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Base class for handling a character standing up
    /// </summary>
    public abstract class PlayerStandHandlerBase<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Sit;

        public PlayerStandHandlerBase(IPlayerInfoProvider playerInfoProvider,
                                      ICharacterRepository characterRepository,
                                      ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
        }

        protected void Handle(int playerId, int x, int y)
        {
            if (playerId == _characterRepository.MainCharacter.ID)
            {
                var renderProperties = _characterRepository.MainCharacter.RenderProperties;
                var updatedRenderProperties = renderProperties.WithSitState(SitState.Standing)
                    .WithCurrentAction(CharacterActionState.Standing)
                    .WithMapX(x)
                    .WithMapY(y);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(updatedRenderProperties);
            }
            else if (_currentMapStateRepository.Characters.TryGetValue(playerId, out var oldCharacter))
            {
                var renderProperties = oldCharacter.RenderProperties.WithSitState(SitState.Standing)
                    .WithCurrentAction(CharacterActionState.Standing)
                    .WithMapX(x)
                    .WithMapY(y);

                _currentMapStateRepository.Characters.Update(oldCharacter, oldCharacter.WithRenderProperties(renderProperties));
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerId);
            }
        }
    }
}
