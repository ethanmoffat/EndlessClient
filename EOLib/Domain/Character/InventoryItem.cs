namespace EOLib.Domain.Character
{
    public class InventoryItem : IInventoryItem
    {
        public short ItemID { get; }
        
        public int Amount { get; }

        public InventoryItem(short itemID, int amount)
        {
            ItemID = itemID;
            Amount = amount;
        }

        public IInventoryItem WithAmount(int newAmount)
        {
            return new InventoryItem(ItemID, newAmount);
        }

        public override bool Equals(object obj)
        {
            var other = obj as InventoryItem;
            if (other == null) return false;
            return other.ItemID == ItemID && other.Amount == Amount;
        }

        public override int GetHashCode()
        {
            int hashCode = 1754760722;
            hashCode = hashCode * -1521134295 + ItemID.GetHashCode();
            hashCode = hashCode * -1521134295 + Amount.GetHashCode();
            return hashCode;
        }
    }

    public interface IInventoryItem
    {
        short ItemID { get; }

        int Amount { get; }

        IInventoryItem WithAmount(int newAmount);
    }
}