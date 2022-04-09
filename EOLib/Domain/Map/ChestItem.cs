using EOLib.Domain.Character;

namespace EOLib.Domain.Map
{
    public class ChestItem : InventoryItem
    {
        public int Slot { get; }

        public ChestItem(short itemID, int amount, int slot)
            : base(itemID, amount)
        {
            Slot = slot;
        }

        public override bool Equals(object obj)
        {
            var chestItem = obj as ChestItem;
            return base.Equals(chestItem) && Slot == chestItem.Slot;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * -1521134295 + Slot.GetHashCode();
        }
    }
}
