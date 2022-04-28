using Amadevus.RecordGenerator;

namespace EOLib.Domain.Online
{
    [Record]
    public sealed partial class OnlinePlayerInfo
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
    }
}
