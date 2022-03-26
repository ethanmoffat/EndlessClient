using AutomaticTypeMapper;
using EndlessClient.HUD.Panels;
using EOLib.IO;
using EOLib.IO.Extensions;
using Optional;

namespace EndlessClient.HUD.Inventory
{
    [AutoMappedType]
    public class InventoryService : IInventoryService
    {
        public Option<int> GetNextOpenSlot(bool[,] usedSlots, ItemSize size, Option<int> preferredSlot)
        {
            var (sizeWidth, sizeHeight) = size.GetDimensions();

            var preferredSlotIsValid = preferredSlot.Match(
                some: slot => IsSlotOpen(usedSlots, slot, size),
                none: () => false);

            if (preferredSlotIsValid)
                return preferredSlot;

            for (int r = 0; r < usedSlots.GetLength(0); r++)
            {
                for (int c = 0; c < usedSlots.GetLength(1); c++)
                {
                    var slot = r * InventoryPanel.InventoryRowSlots + c;
                    if (!usedSlots[r, c] && IsSlotOpen(usedSlots, slot, size))
                        return Option.Some(slot);
                }
            }

            return Option.None<int>();
        }

        public void SetSlots(bool[,] usedSlots, int slot, ItemSize size)
        {
            SetSlotValue(usedSlots, slot, size, value: true);
        }

        public void ClearSlots(bool[,] usedSlots, int slot, ItemSize size)
        {
            SetSlotValue(usedSlots, slot, size, value: false);
        }

        private bool IsSlotOpen(bool[,] usedSlots, int slot, ItemSize size)
        {
            var (sizeWidth, sizeHeight) = size.GetDimensions();

            var col = slot % InventoryPanel.InventoryRowSlots;
            var row = slot / InventoryPanel.InventoryRowSlots;
            for (int r = row; r < row + sizeHeight; r++)
            {
                for (int c = col; c < col + sizeWidth; c++)
                {
                    if (r >= usedSlots.GetLength(0) || c >= usedSlots.GetLength(1) || usedSlots[r, c])
                        return false;
                }
            }

            return true;
        }

        private void SetSlotValue(bool[,] usedSlots, int slot, ItemSize size, bool value)
        {
            var (sizeWidth, sizeHeight) = size.GetDimensions();

            var slotCol = slot % InventoryPanel.InventoryRowSlots;
            var slotRow = slot / InventoryPanel.InventoryRowSlots;

            for (int r = slotRow; r < slotRow + sizeHeight; r++)
            {
                for (int c = slotCol; c < slotCol + sizeWidth; c++)
                {
                    if (r < usedSlots.GetLength(0) && c < usedSlots.GetLength(1))
                        usedSlots[r, c] = value;
                }
            }
        }
    }

    public interface IInventoryService
    {
        Option<int> GetNextOpenSlot(bool[,] usedSlots, ItemSize size, Option<int> preferredSlot);
        void SetSlots(bool[,] usedSlots, int slot, ItemSize size);

        void ClearSlots(bool[,] usedSlots, int slot, ItemSize size);
    }
}
