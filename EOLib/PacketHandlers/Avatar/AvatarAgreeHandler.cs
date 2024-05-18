using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Avatar
{
    /// <summary>
    /// Sent when a character's render properties are changed
    /// </summary>
    [AutoMappedType]
    public class AvatarAgreeHandler : AvatarChangeHandler<AvatarAgreeServerPacket>
    {
        protected readonly ICurrentMapStateRepository _currentMapStateRepository;
        protected readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.Avatar;

        public override PacketAction Action => PacketAction.Agree;

        public AvatarAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICurrentMapStateRepository currentMapStateRepository,
                                  ICharacterRepository characterRepository)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _characterRepository = characterRepository;
        }

        public override bool HandlePacket(AvatarAgreeServerPacket packet)
        {
            Handle(packet.Change);
            return true;
        }
    }
}
