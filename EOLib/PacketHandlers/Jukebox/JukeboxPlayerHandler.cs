using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Jukebox
{
    /// <summary>
    /// Sent when background music should be changed (currently only in weddings)
    /// </summary>
    [AutoMappedType]
    public class JukeboxPlayerHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.JukeBox;

        public override PacketAction Action => PacketAction.Player;

        public JukeboxPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                    IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadChar();
            foreach (var notifier in _soundNotifiers)
                notifier.NotifyMusic(id, isJukebox: false);
            return true;
        }
    }
}
