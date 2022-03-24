using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Repositories;

namespace EndlessClient.HUD.Inventory
{
    [AutoMappedType]
    public class InventorySpaceValidator : IInventorySpaceValidator
    {
        private readonly IEIFFileProvider _eifFileProvider;

        public InventorySpaceValidator(IEIFFileProvider eifFileProvider)
        {
            _eifFileProvider = eifFileProvider;
        }

        public bool ItemFits(IItem item)
        {
            return ItemFits(_eifFileProvider.EIFFile[item.ItemID].Size);
        }

        public bool ItemFits(ItemSize itemSize)
        {
            // todo: inventory grid management
            return true;
        }
    }

    public interface IInventorySpaceValidator
    {
        bool ItemFits(IItem item);

        bool ItemFits(ItemSize itemSize);
    }
}
