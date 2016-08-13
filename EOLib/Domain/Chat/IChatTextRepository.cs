// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Chat
{
    public interface IChatTextRepository
    {
        string ChatText { get; set; }

        string PreviousText { get; set; }
    }

    public interface IChatTextProvider
    {
        string ChatText { get; }

        string PreviousText { get; }
    }

    public class ChatTextRepository : IChatTextRepository, IChatTextProvider
    {
        public string ChatText { get; set; }

        public string PreviousText { get; set; }

        public ChatTextRepository()
        {
            ChatText = "";
            PreviousText = "";
        }
    }
}
