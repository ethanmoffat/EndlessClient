using AutomaticTypeMapper;
using EOLib.Domain.Interact.Jukebox;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Jukebox
{
    [AutoMappedType]
    public class JukeboxOpenHandler : InGameOnlyPacketHandler<JukeboxOpenServerPacket>
    {
        private readonly IJukeboxRepository _jukeboxRepository;
        private readonly IEnumerable<IUserInterfaceNotifier> _userInterfaceNotifiers;

        public override PacketFamily Family => PacketFamily.Jukebox;

        public override PacketAction Action => PacketAction.Open;

        public JukeboxOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                  IJukeboxRepository jukeboxRepository,
                                  IEnumerable<IUserInterfaceNotifier> userInterfaceNotifiers)
            : base(playerInfoProvider)
        {
            _jukeboxRepository = jukeboxRepository;
            _userInterfaceNotifiers = userInterfaceNotifiers;
        }

        public override bool HandlePacket(JukeboxOpenServerPacket packet)
        {
            _jukeboxRepository.PlayingRequestName = packet.JukeboxPlayer.SomeWhen(x => !string.IsNullOrWhiteSpace(x));

            foreach (var notifier in _userInterfaceNotifiers)
                notifier.NotifyPacketDialog(PacketFamily.Jukebox);

            return true;
        }
    }
}