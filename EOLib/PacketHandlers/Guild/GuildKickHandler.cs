using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System.Collections.Generic;

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
            //  _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithGuildTag(string.Empty); doesn't work here?
            return true;
        }
    }
}
