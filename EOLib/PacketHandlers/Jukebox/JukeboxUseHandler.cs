using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Interact.Jukebox;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Jukebox
{
    /// <summary>
    /// Sent when a Jukebox track should be played
    /// </summary>
    [AutoMappedType]
    public class JukeboxUseHandler : InGameOnlyPacketHandler<JukeboxUseServerPacket>
    {
        private readonly IJukeboxRepository _jukeboxRepository;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.Jukebox;

        public override PacketAction Action => PacketAction.Use;

        public JukeboxUseHandler(IPlayerInfoProvider playerInfoProvider,
                                 IJukeboxRepository jukeboxRepository,
                                 IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _jukeboxRepository = jukeboxRepository;
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(JukeboxUseServerPacket packet)
        {
            _jukeboxRepository.PlayingRequestName = Option.Some(string.Empty);

            foreach (var notifier in _soundNotifiers)
                notifier.NotifyMusic(packet.TrackId, isJukebox: true);

            return true;
        }
    }
}
