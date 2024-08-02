using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Message
{
    /// <summary>
    /// Shows a message dialog (ScrollingListDialogSize.Large, ScrollingListDialog.ListItemStyle.Small)
    /// </summary>
    [AutoMappedType]
    public class MessageAcceptHandler : InGameOnlyPacketHandler<MessageAcceptServerPacket>
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

        public override bool HandlePacket(MessageAcceptServerPacket packet)
        {
            foreach (var notifier in _userInterfaceNotifiers)
                notifier.NotifyMessageDialog(packet.Messages[0], packet.Messages.Skip(1).ToList());

            return true;
        }
    }
}