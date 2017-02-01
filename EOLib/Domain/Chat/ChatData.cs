// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Chat
{
    public class ChatData
    {
        public ChatIcon Icon { get; }

        public string Who { get; }

        public string Message { get; }

        public ChatColor ChatColor { get; }

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
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChatData)) return false;
            var other = (ChatData) obj;

            return other.Icon.Equals(Icon)
                   && other.Who.Equals(Who)
                   && other.Message.Equals(Message)
                   && other.ChatColor.Equals(ChatColor);
        }

        public override int GetHashCode()
        {
            var hash = 397 ^ Icon.GetHashCode();
            hash = (hash*397) ^ Who.GetHashCode();
            hash = (hash*397) ^ Message.GetHashCode();
            hash = (hash*397) ^ ChatColor.GetHashCode();
            return hash;
        }
    }
}