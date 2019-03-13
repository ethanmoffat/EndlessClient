// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.IO.Map;
using EOLib.IO.Repositories;

namespace EOLib.Domain.Map
{
    [AutoMappedType]
    public class UnlockDoorValidator : IUnlockDoorValidator
    {
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEIFFileProvider _eifFileProvider;

        public UnlockDoorValidator(ICharacterInventoryProvider characterInventoryProvider,
                              IEIFFileProvider eifFileProvider)
        {
            _characterInventoryProvider = characterInventoryProvider;
            _eifFileProvider = eifFileProvider;
        }

        public bool CanMainCharacterOpenDoor(IWarp warp)
        {
            var itemName = GetRequiredKey(warp);

            return !itemName.HasValue || _characterInventoryProvider.ItemInventory
                       .Select(x => _eifFileProvider.EIFFile[x.ItemID].Name)
                       .Any(x => string.Compare(x, itemName, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public Optional<string> GetRequiredKey(IWarp warp)
        {
            switch (warp.DoorType)
            {
                case DoorSpec.LockedSilver: return "Silver Key";
                case DoorSpec.LockedCrystal: return "Crystal Key";
                case DoorSpec.LockedWraith: return "Wraith Key";
                default: return Optional<string>.Empty;
            }
        }
    }

    public interface IUnlockDoorValidator
    {
        bool CanMainCharacterOpenDoor(IWarp warp);

        Optional<string> GetRequiredKey(IWarp warp);
    }
}
