using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Message
{
    /// <summary>
    /// Shows a message dialog (ScrollingListDialogSize.Large, ScrollingListDialog.ListItemStyle.Small)
    /// </summary>
    [AutoMappedType]
    public class MessageAcceptHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IUserInterfaceNotifier> _userInterfaceNotifiers;

        public override PacketFamily Family => PacketFamily.Message;

        public override PacketAction Action => PacketAction.Accept;

        public MessageAcceptHandler(IPlayerInfoProvider playerInfoProvider,
                                    IEnumerable<IUserInterfaceNotifier> userInterfaceNotifiers)
            : base(playerInfoProvider)
        {
            _userInterfaceNotifiers = userInterfaceNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var title = packet.ReadBreakString();
            var messages = new List<string>();
            while (packet.ReadPosition < packet.Length)
                messages.Add(packet.ReadBreakString());

            foreach (var notifier in _userInterfaceNotifiers)
                notifier.NotifyMessageDialog(title, messages);

            return true;
        }
    }
}
