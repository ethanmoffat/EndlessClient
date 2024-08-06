using Amadevus.RecordGenerator;
using EOLib.IO.Map;

namespace EOLib.Domain.Interact
{
    [Record]
    public sealed partial class TileSpecMapEntity : IMapEntity
    {
        public int X { get; }

        public int Y { get; }
    }
}
