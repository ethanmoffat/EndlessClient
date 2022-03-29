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

        public override bool Equals(object obj)
        {
            var other = obj as InventorySpell;
            if (other == null) return false;
            return other.ID == ID && other.Level == Level;
        }

        public override int GetHashCode()
        {
            int hashCode = 1754760722;
            hashCode = hashCode * -1521134295 + ID.GetHashCode();
            hashCode = hashCode * -1521134295 + Level.GetHashCode();
            return hashCode;
        }
    }

    public interface IInventorySpell
    {
        short ID { get; }

        short Level { get; }

        IInventorySpell WithLevel(short newLevel);
    }
}