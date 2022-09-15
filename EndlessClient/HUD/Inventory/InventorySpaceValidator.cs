using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Optional;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

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

        public bool ItemFits(MapItem item)
        {
            return _characterInventoryProvider.ItemInventory.Any(x => x.ItemID == item.ItemID) || ItemFits(item.ItemID);
        }

        public bool ItemFits(int itemId)
        {
            return _characterInventoryProvider.ItemInventory.Any(x => x.ItemID == itemId) || ItemFits(_eifFileProvider.EIFFile[itemId].Size);
        }

        public bool ItemsFit(IReadOnlyList<InventoryItem> outItems, IReadOnlyList<InventoryItem> inItems)
        {
            return true;
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
        bool ItemFits(MapItem item);

        bool ItemFits(int itemId);

        bool ItemsFit(IReadOnlyList<InventoryItem> outItems, IReadOnlyList<InventoryItem> inItems);
    }
}
