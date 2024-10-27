using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]
    public class GuildAcceptHandler : InGameOnlyPacketHandler<GuildAcceptServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Accept;

        public GuildAcceptHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
        }

        public override bool HandlePacket(GuildAcceptServerPacket packet)
        {
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithGuildRankID(packet.Rank);
            return true;
        }
    }
}
