using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Music
{
    /// <summary>
    /// Sent by the server when a sound effect should be played
    /// </summary>
    [AutoMappedType]
    public class MusicPlayerHandler : InGameOnlyPacketHandler<MusicPlayerServerPacket>
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

        public override bool HandlePacket(MusicPlayerServerPacket packet)
        {
            foreach (var notifier in _notifiers)
                notifier.NotifySoundEffect(packet.SoundId);
            return true;
        }
    }
}
