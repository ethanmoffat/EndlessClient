namespace EOLib.Domain.Online
{
    public class OnlinePlayerInfo
    {
        public string Name { get; }

        public string Title { get; }

        public string Guild { get; }

        public string Class { get; }

        public OnlineIcon Icon { get; }

        public OnlinePlayerInfo(string name)
            : this(name, string.Empty, string.Empty, string.Empty, OnlineIcon.Normal)
        {
        }

        public OnlinePlayerInfo(string name, string title, string guild, string @class, OnlineIcon icon)
        {
            Name = name;
            Title = title;
            Guild = guild;
            Class = @class;
            Icon = icon;
        }
    }
}
