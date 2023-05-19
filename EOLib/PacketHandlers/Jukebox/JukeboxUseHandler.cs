using AutomaticTypeMapper;
using EOLib.Domain.Interact.Jukebox;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Jukebox
{
    /// <summary>
    /// Sent when a Jukebox track should be played
    /// </summary>
    [AutoMappedType]
    public class JukeboxUseHandler : InGameOnlyPacketHandler
    {
        private readonly IJukeboxRepository _jukeboxRepository;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.JukeBox;

        public override PacketAction Action => PacketAction.Use;

        public JukeboxUseHandler(IPlayerInfoProvider playerInfoProvider,
                                 IJukeboxRepository jukeboxRepository,
                                 IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _jukeboxRepository = jukeboxRepository;
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();

            _jukeboxRepository.PlayingRequestName = Option.Some(string.Empty);

            foreach (var notifier in _soundNotifiers)
                notifier.NotifyMusic(id, isJukebox: true);

            return true;
        }
    }
}
