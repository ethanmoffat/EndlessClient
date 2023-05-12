using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Face
{
    /// <summary>
    /// Player changing direction
    /// </summary>
    [AutoMappedType]
    public class FacePlayerHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _mapStateRepository;

        public override PacketFamily Family => PacketFamily.Face;

        public override PacketAction Action => PacketAction.Player;

        public FacePlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICurrentMapStateRepository mapStateRepository)
            : base(playerInfoProvider)
        {
            _mapStateRepository = mapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();
            var direction = (EODirection)packet.ReadChar();

            if (!_mapStateRepository.Characters.ContainsKey(id))
            {
                _mapStateRepository.UnknownPlayerIDs.Add(id);
                return true;
            }

            var character = _mapStateRepository.Characters[id];

            var newRenderProps = character.RenderProperties.WithDirection(direction);
            var newCharacter = character.WithRenderProperties(newRenderProps);

            _mapStateRepository.Characters.Update(character, newCharacter);

            return true;
        }
    }
}
