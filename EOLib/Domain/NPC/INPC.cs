using EOLib.Domain.Spells;
using Optional;

namespace EOLib.Domain.NPC
{
    public interface INPC : ISpellTargetable
    {
        byte X { get; }

        byte Y { get; }

        EODirection Direction { get; }

        NPCFrame Frame { get; }

        Option<short> OpponentID { get; }

        INPC WithX(byte x);

        INPC WithY(byte y);

        INPC WithDirection(EODirection direction);

        INPC WithFrame(NPCFrame frame);

        INPC WithOpponentID(short opponentID);
    }
}