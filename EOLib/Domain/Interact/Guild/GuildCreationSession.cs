using System.Collections.Generic;
using Amadevus.RecordGenerator;

namespace EOLib.Domain.Interact.Guild
{
    [Record]
    public sealed partial class GuildCreationSession
    {
        public string Tag { get; }

        public string Name { get; }

        public string Description { get; }

        public IReadOnlyCollection<string> Members { get; } = new HashSet<string>();

        public bool Approved { get; }
    }
}
