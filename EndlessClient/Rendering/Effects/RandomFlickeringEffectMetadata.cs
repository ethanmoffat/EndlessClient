using Amadevus.RecordGenerator;
using Newtonsoft.Json;

namespace EndlessClient.Rendering.Effects
{
    [Record]
    public sealed partial class RandomFlickeringEffectMetadata
    {
        [JsonProperty("firstFrame")]
        public int FirstFrame { get; }

        [JsonProperty("lastFrame")]
        public int LastFrame { get; }
    }
}