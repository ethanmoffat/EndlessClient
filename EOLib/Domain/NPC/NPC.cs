using System.Collections.Generic;

namespace EOLib.Domain.NPC
{
    public class NPC : INPC
    {
        public short ID { get; }

        public byte Index { get; }

        public byte X { get; private set; }

        public byte Y { get; private set; }

        public EODirection Direction { get; private set; }

        public NPCFrame Frame { get; private set; }

        public Optional<short> OpponentID { get; private set; }

        public NPC(short id, byte index)
        {
            ID = id;
            Index = index;
        }

        public INPC WithX(byte x)
        {
            var copy = MakeCopy(this);
            copy.X = x;
            return copy;
        }

        public INPC WithY(byte y)
        {
            var copy = MakeCopy(this);
            copy.Y = y;
            return copy;
        }

        public INPC WithDirection(EODirection direction)
        {
            var copy = MakeCopy(this);
            copy.Direction = direction;
            return copy;
        }

        public INPC WithFrame(NPCFrame frame)
        {
            var copy = MakeCopy(this);
            copy.Frame = frame;
            return copy;
        }

        public INPC WithOpponentID(Optional<short> opponentID)
        {
            var copy = MakeCopy(this);
            copy.OpponentID = opponentID;
            return copy;
        }

        private static NPC MakeCopy(INPC input)
        {
            return new NPC(input.ID, input.Index)
            {
                X = input.X,
                Y = input.Y,
                Direction = input.Direction,
                Frame = input.Frame,
                OpponentID = input.OpponentID
            };
        }

        public override bool Equals(object obj)
        {
            return obj is NPC npc &&
                   ID == npc.ID &&
                   Index == npc.Index &&
                   X == npc.X &&
                   Y == npc.Y &&
                   Direction == npc.Direction &&
                   Frame == npc.Frame &&
                   EqualityComparer<Optional<short>>.Default.Equals(OpponentID, npc.OpponentID);
        }

        public override int GetHashCode()
        {
            int hashCode = 137608487;
            hashCode = hashCode * -1521134295 + ID.GetHashCode();
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Direction.GetHashCode();
            hashCode = hashCode * -1521134295 + Frame.GetHashCode();
            hashCode = hashCode * -1521134295 + OpponentID.GetHashCode();
            return hashCode;
        }
    }
}