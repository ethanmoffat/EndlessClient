using Amadevus.RecordGenerator;

namespace EndlessClient.Rendering.NPC
{
    [Record]
    public sealed partial class NPCFrameMetadata
    {
        public int OffsetX { get; }
        public int OffsetY { get; }

        public int AttackOffsetX { get; }
        public int AttackOffsetY { get; }

        public bool HasStandingFrameAnimation { get; }

        public int NameLabelOffset { get; }
    }
}
