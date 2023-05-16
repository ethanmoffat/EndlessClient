using Amadevus.RecordGenerator;
using EOLib.IO.Map;

namespace EOLib.Domain.Interact.Board
{
    [Record]
    public sealed partial class BoardMapEntity : IMapEntity
    {
        public int X { get; }

        public int Y { get; }
    }
}
