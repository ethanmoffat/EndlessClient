namespace EOLib.Domain.NPC
{
    public interface INPC
    {
        short ID { get; }

        byte Index { get; }

        byte X { get; }

        byte Y { get; }

        EODirection Direction { get; }

        NPCFrame Frame { get; }

        Optional<short> OpponentID { get; }

        INPC WithX(byte x);

        INPC WithY(byte y);

        INPC WithDirection(EODirection direction);

        INPC WithFrame(NPCFrame frame);

        INPC WithOpponentID(Optional<short> opponentID);
    }
}