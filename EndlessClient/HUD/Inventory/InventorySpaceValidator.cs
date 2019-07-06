// Original Work Copyright (c) Ethan Moffat 2014-2019

using AutomaticTypeMapper;
using EOLib.Domain.Map;
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
            var itemSize = _eifFileProvider.EIFFile[item.ItemID].Size;
            // todo: inventory grid management
            return true;
        }
    }

    public interface IInventorySpaceValidator
    {
        bool ItemFits(IItem item);
    }
}
