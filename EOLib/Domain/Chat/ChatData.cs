using Amadevus.RecordGenerator;
using System;

namespace EOLib.Domain.Chat
{
    [Record(Features.ObjectEquals | Features.ToString)]
    public sealed partial class ChatData
    {
        public ChatTab Tab { get; private set; }

        public ChatIcon Icon { get; private set; }

        public string Who { get; private set; }

        public string Message { get; } // making this get-only makes it the only property that generates a .With method

        public ChatColor ChatColor { get; private set; }

        public DateTime ChatTime { get; private set; }

        public bool Log { get; private set; }

        public ChatData(ChatTab tab,
                        string who,
                        string message,
                        ChatIcon icon = ChatIcon.None,
                        ChatColor chatColor = ChatColor.Default,
                        bool log = true)
        {
            if (who == null)
                who = "";
            else if (who.Length >= 1)
                who = char.ToUpper(who[0]) + who.Substring(1).ToLower();

            if (message == null)
                message = "";

            Tab = tab;
            Icon = icon;
            Who = who;
            Message = message;
            ChatColor = chatColor;
            Log = log;

            ChatTime = DateTime.Now;
        }

        public ChatData WithMessage(string message)
        {
            return new ChatData(Tab, Who, message, Icon, ChatColor, Log);
        }
    }
}