// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Chat
{
    public interface IChatTextRepository
    {
        string LocalTypedText { get; set; }
    }

    public interface IChatTextProvider
    {
        string LocalTypedText { get; }
    }

    public class ChatTextRepository : IChatTextRepository, IChatTextProvider
    {
        public string LocalTypedText { get; set; }

        public ChatTextRepository()
        {
            LocalTypedText = "";
        }
    }
}
