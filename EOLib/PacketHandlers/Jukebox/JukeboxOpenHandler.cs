using AutomaticTypeMapper;
using EOLib.Domain.Interact.Jukebox;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System.Collections.Generic;
using System.IO;

namespace EOLib.PacketHandlers.Jukebox
{
    [AutoMappedType]
    public class JukeboxOpenHandler : InGameOnlyPacketHandler
    {
        private readonly IJukeboxRepository _jukeboxRepository;
        private readonly IEnumerable<IUserInterfaceNotifier> _userInterfaceNotifiers;

        public override PacketFamily Family => PacketFamily.JukeBox;

        public override PacketAction Action => PacketAction.Open;

        public JukeboxOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                  IJukeboxRepository jukeboxRepository,
                                  IEnumerable<IUserInterfaceNotifier> userInterfaceNotifiers)
            : base(playerInfoProvider)
        {
            _jukeboxRepository = jukeboxRepository;
            _userInterfaceNotifiers = userInterfaceNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            packet.Seek(2, SeekOrigin.Current);

            if (packet.ReadPosition < packet.Length)
            {
                _jukeboxRepository.PlayingRequestName = Option.Some(packet.ReadEndString());
            }
            else
            {
                _jukeboxRepository.PlayingRequestName = Option.None<string>();
            }

            foreach (var notifier in _userInterfaceNotifiers)
                notifier.NotifyPacketDialog(PacketFamily.JukeBox);

            return true;
        }
    }
}
