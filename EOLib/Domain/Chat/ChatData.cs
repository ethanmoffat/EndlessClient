using Amadevus.RecordGenerator;
using System;

namespace EOLib.Domain.Chat
{
    [Record(Features.ObjectEquals | Features.ToString)]
    public sealed partial class ChatData
    {
        public ChatIcon Icon { get; }

        public string Who { get; }

        public string Message { get; }

        public ChatColor ChatColor { get; }

        public DateTime ChatTime { get; }

        public ChatData(string who,
            string message,
            ChatIcon icon = ChatIcon.None,
            ChatColor color = ChatColor.Default)
        {
            if (who == null)
                who = "";
            else if (who.Length >= 1)
                who = char.ToUpper(who[0]) + who.Substring(1).ToLower();

            if (message == null)
                message = "";

            Icon = icon;
            Who = who;
            Message = message;
            ChatColor = color;

            ChatTime = DateTime.Now;
        }
    }
}