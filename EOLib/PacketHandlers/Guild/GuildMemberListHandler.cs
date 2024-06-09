using System.Diagnostics;
using AutomaticTypeMapper;
using EOLib.Domain.Interact.Guild;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]
    public class GuildMemberListHandler : InGameOnlyPacketHandler<GuildTellServerPacket>
    {
        private readonly IGuildSessionRepository _guildSessionRepository;
        private readonly IGuildSessionProvider _guildSessionProvider;

        public override PacketFamily Family => PacketFamily.Guild;
        public override PacketAction Action => PacketAction.Tell;

        public GuildMemberListHandler(IPlayerInfoProvider playerInfoProvider,
                                      IGuildSessionRepository guildSessionRepository,
                                      IGuildSessionProvider guildSessionProvider)
            : base(playerInfoProvider)
        {
            _guildSessionRepository = guildSessionRepository;
            _guildSessionProvider = guildSessionProvider;
        }

        public override bool HandlePacket(GuildTellServerPacket packet)
        {
            
            _guildSessionRepository.Names.Clear();
            foreach (var member in packet.Members)
            {
                Debug.WriteLine($"{_guildSessionProvider.Names.Count} Called from Member List Handler");
                _guildSessionRepository.Names.Add(member.Name);
            }

            _guildSessionRepository.OnMemberListUpdated();

            return true;
        }
    }
}
