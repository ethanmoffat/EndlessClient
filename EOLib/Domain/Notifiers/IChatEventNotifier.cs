// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Notifiers
{
    public interface IChatEventNotifier
    {
        void NotifyPrivateMessageRecipientNotFound(string recipientName);

        void NotifyPlayerMutedByAdmin(string adminName);
    }

    public class NoOpChatEventNotifier : IChatEventNotifier
    {
        public void NotifyPrivateMessageRecipientNotFound(string recipientName) { }

        public void NotifyPlayerMutedByAdmin(string adminName) { }
    }
}
