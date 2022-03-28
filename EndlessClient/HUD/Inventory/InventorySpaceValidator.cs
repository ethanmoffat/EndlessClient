using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Optional;

namespace EndlessClient.HUD.Inventory
{
    [AutoMappedType]
    public class InventorySpaceValidator : IInventorySpaceValidator
    {
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IInventorySlotProvider _inventorySlotProvider;
        private readonly IInventoryService _inventoryService;

        public InventorySpaceValidator(IEIFFileProvider eifFileProvider,
                                       IInventorySlotProvider inventorySlotProvider,
                                       IInventoryService inventoryService)
        {
            _eifFileProvider = eifFileProvider;
            _inventorySlotProvider = inventorySlotProvider;
            _inventoryService = inventoryService;
        }

        public bool ItemFits(IItem item)
        {
            return ItemFits(_eifFileProvider.EIFFile[item.ItemID].Size);
        }

        public bool ItemFits(ItemSize itemSize)
        {
            return _inventoryService
                .GetNextOpenSlot((Matrix<bool>)_inventorySlotProvider.FilledSlots, itemSize, Option.None<int>())
                .HasValue;
        }
    }

    public interface IInventorySpaceValidator
    {
        bool ItemFits(IItem item);

        bool ItemFits(ItemSize itemSize);

        // need "ItemsFit" method for trading
    }
}
