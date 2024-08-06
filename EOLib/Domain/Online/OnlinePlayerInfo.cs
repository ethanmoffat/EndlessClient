using Amadevus.RecordGenerator;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Online
{
    [Record]
    public sealed partial class OnlinePlayerInfo
    {
        public string Name { get; }

        public string Title { get; }

        public string Guild { get; }

        public string Class { get; }

        public CharacterIcon Icon { get; }

        public OnlinePlayerInfo(string name)
            : this(name, string.Empty, string.Empty, string.Empty, CharacterIcon.Player)
        {
        }
    }
}
