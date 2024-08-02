using System.Collections.Generic;
using Amadevus.RecordGenerator;
using Newtonsoft.Json;

namespace EndlessClient.Rendering.Effects
{
    [Record]
    public sealed partial class PositionOffsetEffectMetadata
    {
        [JsonProperty("xOffsetPerFrame")]
        public IReadOnlyList<int> OffsetXByFrame { get; }

        [JsonProperty("yOffsetPerFrame")]
        public IReadOnlyList<int> OffsetYByFrame { get; }
    }
}