using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class MusicPlayerHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<ISoundNotifier> _notifiers;

        public override PacketFamily Family => PacketFamily.Music;

        public override PacketAction Action => PacketAction.Player;

        public MusicPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                  IEnumerable<ISoundNotifier> notifiers)
            : base(playerInfoProvider)
        {
            _notifiers = notifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var sfxId = packet.ReadChar();
            foreach (var notifier in _notifiers)
                notifier.NotifySoundEffect(sfxId);
            return true;
        }
    }
}
