using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface IChatEventNotifier
    {
        void NotifyPrivateMessageRecipientNotFound(string recipientName);

        void NotifyPlayerMutedByAdmin(string adminName);
    }

    [AutoMappedType]
    public class NoOpChatEventNotifier : IChatEventNotifier
    {
        public void NotifyPrivateMessageRecipientNotFound(string recipientName) { }

        public void NotifyPlayerMutedByAdmin(string adminName) { }
    }
}
