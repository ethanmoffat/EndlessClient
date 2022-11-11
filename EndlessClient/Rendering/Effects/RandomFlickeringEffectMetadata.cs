using Amadevus.RecordGenerator;

namespace EndlessClient.Rendering.Effects
{
    [Record]
    public sealed partial class RandomFlickeringEffectMetadata
    {
        public int FirstFrame { get; }

        public int LastFrame { get; }
    }
}
