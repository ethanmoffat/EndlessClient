using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    public abstract class PlayerStandHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Sit;

        public PlayerStandHandler(IPlayerInfoProvider playerInfoProvider,
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

            if (playerId == _characterRepository.MainCharacter.ID)
            {
                var renderProperties = _characterRepository.MainCharacter.RenderProperties;
                var updatedRenderProperties = renderProperties.WithSitState(SitState.Standing)
                    .WithCurrentAction(CharacterActionState.Standing)
                    .WithMapX(x)
                    .WithMapY(y);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(updatedRenderProperties);
            }
            else if (_currentMapStateRepository.Characters.ContainsKey(playerId))
            {
                var oldCharacter = _currentMapStateRepository.Characters[playerId];
                var renderProperties = oldCharacter.RenderProperties.WithSitState(SitState.Standing)
                    .WithCurrentAction(CharacterActionState.Standing)
                    .WithMapX(x)
                    .WithMapY(y);

                _currentMapStateRepository.Characters[playerId] = oldCharacter.WithRenderProperties(renderProperties);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerId);
            }

            return true;
        }
    }

    [AutoMappedType]
    public class OtherPlayerStandHandler : PlayerStandHandler
    {
        public override PacketAction Action => PacketAction.Remove;

        public OtherPlayerStandHandler(IPlayerInfoProvider playerInfoProvider,
                                       ICharacterRepository characterRepository,
                                       ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }
    }

    [AutoMappedType]
    public class MainPlayerStandHandler : PlayerStandHandler
    {
        public override PacketAction Action => PacketAction.Close;

        public MainPlayerStandHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICharacterRepository characterRepository,
                                      ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }
    }

    [AutoMappedType]
    public class MainPlayerStandFromChairHandler : MainPlayerStandHandler
    {
        public override PacketFamily Family => PacketFamily.Chair;

        public MainPlayerStandFromChairHandler(IPlayerInfoProvider playerInfoProvider,
                                               ICharacterRepository characterRepository,
                                               ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }
    }
}
