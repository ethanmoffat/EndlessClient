using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Linq;

namespace EOLib.PacketHandlers
{
    public abstract class PlayerSitHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketAction Action => PacketAction.Player;

        public PlayerSitHandler(IPlayerInfoProvider playerInfoProvider,
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
                    .WithMapX(x)
                    .WithMapY(y)
                    .WithDirection(direction);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(updatedRenderProperties);
            }
            else
            {
                var oldCharacter = _currentMapStateRepository.Characters.Single(c => c.ID == playerId);
                var renderProperties = oldCharacter.RenderProperties.WithSitState(sitState)
                    .WithMapX(x)
                    .WithMapY(y)
                    .WithDirection(direction);
                var newCharacter = oldCharacter.WithRenderProperties(renderProperties);
                
                _currentMapStateRepository.Characters.Remove(oldCharacter);
                _currentMapStateRepository.Characters.Add(newCharacter);
            }

            return true;
        }
    }

    [AutoMappedType]
    public class PlayerSitFloorHandler : PlayerSitHandler
    {
        public override PacketFamily Family => PacketFamily.Sit;

        public PlayerSitFloorHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICharacterRepository characterRepository,
                                     ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }
    }

    [AutoMappedType]
    public class PlayerSitChairHandler : PlayerSitHandler
    {
        public override PacketFamily Family => PacketFamily.Chair;

        public PlayerSitChairHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICharacterRepository characterRepository,
                                     ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository) { }
    }
}
