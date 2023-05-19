using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Jukebox
{
    /// <summary>
    /// Sent when a Jukebox track should be played
    /// </summary>
    [AutoMappedType]
    public class JukeboxUseHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.JukeBox;

        public override PacketAction Action => PacketAction.Use;

        public JukeboxUseHandler(IPlayerInfoProvider playerInfoProvider,
                                 IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();
            foreach (var notifier in _soundNotifiers)
                notifier.NotifyMusic(id, isJukebox: true);
            return true;
        }
    }
}
