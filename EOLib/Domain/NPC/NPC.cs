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
    }
}