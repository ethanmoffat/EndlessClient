using Amadevus.RecordGenerator;
using EndlessClient.Audio;
using Newtonsoft.Json;

namespace EndlessClient.Rendering.Effects
{
    [Record]
    public sealed partial class EffectMetadata
    {
        [JsonProperty("hasLayer0")]
        public bool HasBehindLayer { get; }

        [JsonProperty("hasLayer1")]
        public bool HasTransparentLayer { get; }

        [JsonProperty("hasLayer2")]
        public bool HasInFrontLayer { get; }

        [JsonProperty("sfx")]
        public SoundEffectID SoundEffect { get; }

        [JsonProperty("frames")]
        public int Frames { get; }

        [JsonProperty("loops")]
        public int Loops { get; }

        [JsonProperty("xOffset")]
        public int OffsetX { get; }

        [JsonProperty("yOffset")]
        public int OffsetY { get; }

        [JsonProperty("type")]
        public EffectAnimationType AnimationType { get; }

        [JsonProperty("verticalSlidingData")]
        public VerticalSlidingEffectMetadata VerticalSlidingMetadata { get; }

        [JsonProperty("positionData")]
        public PositionOffsetEffectMetadata PositionOffsetMetadata { get; }

        [JsonProperty("flickeringData")]
        public RandomFlickeringEffectMetadata RandomFlickeringMetadata { get; }
    }
}
