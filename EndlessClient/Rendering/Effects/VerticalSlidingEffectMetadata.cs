using Amadevus.RecordGenerator;
using Newtonsoft.Json;

namespace EndlessClient.Rendering.Effects
{
    [Record]
    public sealed partial class VerticalSlidingEffectMetadata
    {
        [JsonProperty("yOffsetPerFrame")]
        public int FrameOffsetY { get; }
    }
}
