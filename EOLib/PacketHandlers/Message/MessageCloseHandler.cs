using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Message
{
    /// <summary>
    /// Shows the server reboot message
    /// </summary>
    [AutoMappedType]
    public class MessageCloseHandler : InGameOnlyPacketHandler<MessageCloseServerPacket>
    {
        private readonly IEnumerable<IServerRebootNotifier> _serverRebootNotifiers;

        public override PacketFamily Family => PacketFamily.Message;

        public override PacketAction Action => PacketAction.Close;

        public MessageCloseHandler(IPlayerInfoProvider playerInfoProvider,
                                    IEnumerable<IServerRebootNotifier> serverRebootNotifiers)
            : base(playerInfoProvider)
        {
            _serverRebootNotifiers = serverRebootNotifiers;
        }


        public override bool HandlePacket(MessageCloseServerPacket packet)
        {
            foreach (var notifier in _serverRebootNotifiers)
                notifier.NotifyServerReboot();

            return true;
        }
    }
}