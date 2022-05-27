using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public enum ChatEventType
    {
        PrivateMessage,
        AdminChat,
        AdminAnnounce,
        Group,
        Server,
    }

    public interface IChatEventNotifier
    {
        void NotifyChatReceived(ChatEventType eventType);

        void NotifyPrivateMessageRecipientNotFound(string recipientName);

        void NotifyPlayerMutedByAdmin(string adminName);
    }

    [AutoMappedType]
    public class NoOpChatEventNotifier : IChatEventNotifier
    {
        public void NotifyChatReceived(ChatEventType eventType) { }

        public void NotifyPrivateMessageRecipientNotFound(string recipientName) { }

        public void NotifyPlayerMutedByAdmin(string adminName) { }
    }
}
