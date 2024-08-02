using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Jukebox
{
    /// <summary>
    /// Sent when background music should be changed (currently only in weddings)
    /// </summary>
    [AutoMappedType]
    public class JukeboxPlayerHandler : InGameOnlyPacketHandler<JukeboxPlayerServerPacket>
    {
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.Jukebox;

        public override PacketAction Action => PacketAction.Player;

        public JukeboxPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                    IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(JukeboxPlayerServerPacket packet)
        {
            foreach (var notifier in _soundNotifiers)
                notifier.NotifyMusic(packet.MfxId, isJukebox: false);
            return true;
        }
    }
}