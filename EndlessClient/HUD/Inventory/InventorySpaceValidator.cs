using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Optional;
using System.Linq;

namespace EndlessClient.HUD.Inventory
{
    [AutoMappedType]
    public class InventorySpaceValidator : IInventorySpaceValidator
    {
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IInventorySlotProvider _inventorySlotProvider;
        private readonly IInventoryService _inventoryService;

        public InventorySpaceValidator(IEIFFileProvider eifFileProvider,
                                       ICharacterInventoryProvider characterInventoryProvider,
                                       IInventorySlotProvider inventorySlotProvider,
                                       IInventoryService inventoryService)
        {
            _eifFileProvider = eifFileProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _inventorySlotProvider = inventorySlotProvider;
            _inventoryService = inventoryService;
        }

        public bool ItemFits(IItem item)
        {
            return _characterInventoryProvider.ItemInventory.Any(x => x.ItemID == item.ItemID) || ItemFits(item.ItemID);
        }

        public bool ItemFits(int itemId)
        {
            return _characterInventoryProvider.ItemInventory.Any(x => x.ItemID == itemId) || ItemFits(_eifFileProvider.EIFFile[itemId].Size);
        }

        private bool ItemFits(ItemSize itemSize)
        {
            return _inventoryService
                .GetNextOpenSlot((Matrix<bool>)_inventorySlotProvider.FilledSlots, itemSize, Option.None<int>())
                .HasValue;
        }
    }

    public interface IInventorySpaceValidator
    {
        bool ItemFits(IItem item);

        bool ItemFits(int itemId);

        // todo: need "ItemsFit" method for trading
    }
}
