namespace EOLib.Domain.Character
{
    public class InventorySpell : IInventorySpell
    {
        public short ID { get; }

        public short Level { get; }

        public InventorySpell(short id, short level)
        {
            ID = id;
            Level = level;
        }

        public IInventorySpell WithLevel(short newLevel)
        {
            return new InventorySpell(ID, newLevel);
        }
    }

    public interface IInventorySpell
    {
        short ID { get; }

        short Level { get; }

        IInventorySpell WithLevel(short newLevel);
    }
}