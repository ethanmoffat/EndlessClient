using Amadevus.RecordGenerator;
using Newtonsoft.Json;

namespace EndlessClient.Rendering.Metadata.Models
{
    [Record]
    public sealed partial class NPCMetadata
    {
        [JsonProperty("xOffset")]
        public int OffsetX { get; }

        [JsonProperty("yOffset")]
        public int OffsetY { get; }

        [JsonProperty("xAttackOffset")]
        public int AttackOffsetX { get; }

        [JsonProperty("yAttackOffset")]
        public int AttackOffsetY { get; }

        [JsonProperty("hasAnimation")]
        public bool HasStandingFrameAnimation { get; }

        [JsonProperty("nameHeight")]
        public int NameLabelOffset { get; }
    }
}
