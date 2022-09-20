using AutomaticTypeMapper;
using EOLib.Net;
using System.Collections.Generic;

namespace EOLib.Domain.Notifiers
{
    public interface IUserInterfaceNotifier
    {
        void NotifyPacketDialog(PacketFamily packetFamily);

        void NotifyMessageDialog(string title, IReadOnlyList<string> messages);
    }

    [AutoMappedType]
    public class NoOpUserInterfaceNotifier : IUserInterfaceNotifier
    {
        public void NotifyPacketDialog(PacketFamily packetFamily) { }

        public void NotifyMessageDialog(string title, IReadOnlyList<string> messages) { }
    }
}
