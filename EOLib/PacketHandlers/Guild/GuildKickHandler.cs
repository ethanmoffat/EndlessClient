using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]

    public class GuildKickHandler : InGameOnlyPacketHandler<GuildKickServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Kick;

        public GuildKickHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
        }

        public override bool HandlePacket(GuildKickServerPacket packet)
        {
            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithGuildTag("   ")
                .WithGuildName(string.Empty)
                .WithGuildRank(string.Empty);
            return true;
        }
    }
}
