using Amadevus.RecordGenerator;
using System.Collections.Generic;

namespace EndlessClient.Rendering.Effects
{
    [Record]
    public sealed partial class PositionOffsetEffectMetadata
    {
        public IReadOnlyList<int> OffsetXByFrame { get; }

        public IReadOnlyList<int> OffsetYByFrame { get; }
    }
}
