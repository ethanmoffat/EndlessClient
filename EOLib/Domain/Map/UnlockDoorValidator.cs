using System;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Optional;

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
            return GetRequiredKey(warp).Match(
                some: keyName => _characterInventoryProvider
                    .ItemInventory
                    .Where(x => _eifFileProvider.EIFFile[x.ItemID].Type == IO.ItemType.Key)
                    .Select(x => _eifFileProvider.EIFFile[x.ItemID].Name)
                    .Any(keyName.Equals),
                none: () => true);
        }

        public Option<string> GetRequiredKey(IWarp warp)
        {
            switch (warp.DoorType)
            {
                case DoorSpec.LockedSilver: return Option.Some("Silver Key");
                case DoorSpec.LockedCrystal: return Option.Some("Crystal Key");
                case DoorSpec.LockedWraith: return Option.Some("Wraith Key");
                default: return Option.None<string>();
            }
        }
    }

    public interface IUnlockDoorValidator
    {
        bool CanMainCharacterOpenDoor(IWarp warp);

        Option<string> GetRequiredKey(IWarp warp);
    }
}
