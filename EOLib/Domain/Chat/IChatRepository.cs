// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Chat
{
    public interface IChatRepository
    {
        string LocalTypedText { get; set; }

        ChatMode CurrentChatMode { get; set; }
    }

    public interface IChatProvider
    {
        string LocalTypedText { get; }

        ChatMode CurrentChatMode { get; }
    }

    public class ChatRepository : IChatRepository, IChatProvider
    {
        public string LocalTypedText { get; set; }

        public ChatMode CurrentChatMode { get; set; }

        public ChatRepository()
        {
            LocalTypedText = "";
            CurrentChatMode = ChatMode.NoText;
        }
    }
}
