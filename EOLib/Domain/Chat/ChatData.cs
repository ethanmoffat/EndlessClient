using System;
using Amadevus.RecordGenerator;

namespace EOLib.Domain.Chat
{
    [Record(Features.ObjectEquals | Features.ToString)]
    public sealed partial class ChatData
    {
        public ChatTab Tab { get; }

        public ChatIcon Icon { get; }

        public string Who { get; }

        public string Message { get; }

        public ChatColor ChatColor { get; }

        public DateTime ChatTime { get; }

        public bool Log { get; }

        public bool Filter { get; }

        public ChatData(ChatTab tab,
                        string who,
                        string message,
                        ChatIcon icon = ChatIcon.None,
                        ChatColor chatColor = ChatColor.Default,
                        bool log = true,
                        bool filter = true)
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
            Filter = filter;

            ChatTime = DateTime.Now;
        }

        public ChatData WithMessage(string message)
        {
            return new ChatData(Tab, Who, message, Icon, ChatColor, Log, Filter);
        }
    }
}
