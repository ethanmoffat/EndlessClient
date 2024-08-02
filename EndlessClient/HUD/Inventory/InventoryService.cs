using System.Collections.Generic;
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
                some: slot => IsSlotOpen(usedSlots, Option.None<int>(), slot, size),
                none: () => false);

            if (preferredSlotIsValid)
                return preferredSlot;

            for (int r = 0; r < usedSlots.GetLength(0); r++)
            {
                for (int c = 0; c < usedSlots.GetLength(1); c++)
                {
                    var slot = r * InventoryPanel.InventoryRowSlots + c;
                    if (!usedSlots[r, c] && IsSlotOpen(usedSlots, Option.None<int>(), slot, size))
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

        public bool FitsInSlot(bool[,] usedSlots, int oldSlot, int newSlot, ItemSize size) =>
            IsSlotOpen(usedSlots, Option.Some(oldSlot), newSlot, size);

        public bool FitsInSlot(bool[,] usedSlots, int newSlot, ItemSize size) =>
            IsSlotOpen(usedSlots, Option.None<int>(), newSlot, size);

        private bool IsSlotOpen(bool[,] usedSlots, Option<int> oldSlot, int slot, ItemSize size)
        {
            var (sizeWidth, sizeHeight) = size.GetDimensions();

            var ignorePoints = new List<(int X, int Y)>();
            oldSlot.MatchSome(s =>
            {
                var oldCol = s % InventoryPanel.InventoryRowSlots;
                var oldRow = s / InventoryPanel.InventoryRowSlots;

                for (int r = oldRow; r < oldRow + sizeHeight; r++)
                    for (int c = oldCol; c < oldCol + sizeWidth; c++)
                        ignorePoints.Add((c, r));
            });

            var col = slot % InventoryPanel.InventoryRowSlots;
            var row = slot / InventoryPanel.InventoryRowSlots;
            for (int r = row; r < row + sizeHeight; r++)
            {
                for (int c = col; c < col + sizeWidth; c++)
                {
                    if (r >= usedSlots.GetLength(0) || c >= usedSlots.GetLength(1) ||
                        r < 0 || c < 0 ||
                        (!ignorePoints.Contains((c, r)) && usedSlots[r, c]))
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

        bool FitsInSlot(bool[,] usedSlots, int oldSlot, int newSlot, ItemSize size);

        bool FitsInSlot(bool[,] usedSlots, int newSlot, ItemSize size);
    }
}