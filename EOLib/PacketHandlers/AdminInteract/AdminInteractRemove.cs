using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.AdminInteract
{
    /// <summary>
    /// Admin hiding
    /// </summary>
    [AutoMappedType]
    public class AdminInteractRemove : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.AdminInteract;
        public override PacketAction Action => PacketAction.Remove;

        public AdminInteractRemove(IPlayerInfoProvider playerInfoProvider,
                                  ICharacterRepository characterRepository,
                                  ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();

            if (id == _characterRepository.MainCharacter.ID)
                _characterRepository.MainCharacter = Hidden(_characterRepository.MainCharacter);
            else
            {
                if (_currentMapStateRepository.Characters.ContainsKey(id))
                {
                    var character = _currentMapStateRepository.Characters[id];

                    var updatedCharacter = Hidden(character);
                    _currentMapStateRepository.Characters[id] = updatedCharacter;
                }
                else
                {
                    _currentMapStateRepository.UnknownPlayerIDs.Add(id);
                }
            }

            return true;
        }

        private static Character Hidden(Character input)
        {
            var renderProps = input.RenderProperties.WithIsHidden(true);
            return input.WithRenderProperties(renderProps);
        }
    }
}
