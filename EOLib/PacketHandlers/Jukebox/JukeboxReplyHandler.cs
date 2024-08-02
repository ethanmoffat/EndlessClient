using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Jukebox
{
    [AutoMappedType]
    public class JukeboxReplyHandler : InGameOnlyPacketHandler<JukeboxReplyServerPacket>
    {
        private readonly IEnumerable<IJukeboxNotifier> _jukeboxNotifiers;

        public override PacketFamily Family => PacketFamily.Jukebox;

        public override PacketAction Action => PacketAction.Reply;

        public JukeboxReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                   IEnumerable<IJukeboxNotifier> jukeboxNotifiers)
            : base(playerInfoProvider)
        {
            _jukeboxNotifiers = jukeboxNotifiers;
        }

        public override bool HandlePacket(JukeboxReplyServerPacket packet)
        {
            foreach (var notifier in _jukeboxNotifiers)
                notifier.JukeboxUnavailable();
            return true;
        }
    }
}