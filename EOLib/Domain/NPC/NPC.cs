using Amadevus.RecordGenerator;
using EOLib.Domain.Spells;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.Domain.NPC
{
    [Record]
    public sealed partial class NPC : ISpellTargetable
    {
        public int ID { get; }

        public int Index { get; }

        public int X { get; }

        public int Y { get; }

        public EODirection Direction { get; }

        public NPCFrame Frame { get; }

        public int ActualAttackFrame { get; }

        public Option<int> OpponentID { get; }

        public static NPC FromNearby(NpcMapInfo npcMapInfo)
        {
            return new Builder
            {
                ID = npcMapInfo.Id,
                Index = npcMapInfo.Index,
                X = npcMapInfo.Coords.X,
                Y = npcMapInfo.Coords.Y,
                Direction = (EODirection)npcMapInfo.Direction,
            }.ToImmutable();
        }
    }
}