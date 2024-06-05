using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Face
{
    /// <summary>
    /// Player changing direction
    /// </summary>
    [AutoMappedType]
    public class FacePlayerHandler : InGameOnlyPacketHandler<FacePlayerServerPacket>
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

        public override bool HandlePacket(FacePlayerServerPacket packet)
        {
            if (!_mapStateRepository.Characters.ContainsKey(packet.PlayerId))
            {
                _mapStateRepository.UnknownPlayerIDs.Add(packet.PlayerId);
                return true;
            }

            var character = _mapStateRepository.Characters[packet.PlayerId];

            var newRenderProps = character.RenderProperties.WithDirection((EODirection)packet.Direction);
            var newCharacter = character.WithRenderProperties(newRenderProps);

            _mapStateRepository.Characters.Update(character, newCharacter);

            return true;
        }
    }
}
