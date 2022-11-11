using Amadevus.RecordGenerator;
using EndlessClient.Audio;

namespace EndlessClient.Rendering.Effects
{
    [Record]
    public sealed partial class EffectMetadata
    {
        public bool HasBehindLayer { get; }

        public bool HasTransparentLayer { get; }

        public bool HasInFrontLayer { get; }

        public SoundEffectID SoundEffect { get; }

        public int Frames { get; }

        public int Loops { get; }

        public int OffsetX { get; }

        public int OffsetY { get; }

        public EffectAnimationType AnimationType { get; }

        public VerticalSlidingEffectMetadata VerticalSlidingMetadata { get; }

        public PositionOffsetEffectMetadata PositionOffsetMetadata { get; }

        public RandomFlickeringEffectMetadata RandomFlickeringMetadata { get; }
    }
}
