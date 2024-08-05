using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.IO.Repositories;
using Optional;
using Optional.Collections;

namespace EOLib.Domain.Map
{
    [AutoMappedType]
    public class UnlockDoorValidator : IUnlockDoorValidator
    {
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IEIFFileProvider _eifFileProvider;

        public UnlockDoorValidator(ICharacterInventoryProvider characterInventoryProvider,
                                   ICurrentMapStateProvider currentMapStateProvider,
                                   IEIFFileProvider eifFileProvider)
        {
            _characterInventoryProvider = characterInventoryProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _eifFileProvider = eifFileProvider;
        }

        public bool CanMainCharacterOpenDoor(Warp warp)
        {
            return (int)warp.DoorType <= 1 || _currentMapStateProvider.OpenDoors.Any(d => d.X == warp.X && d.Y == warp.Y) ||
                _characterInventoryProvider.ItemInventory.Any(x => _eifFileProvider.EIFFile[x.ItemID].Type == IO.ItemType.Key &&
                                                                   _eifFileProvider.EIFFile[x.ItemID].Key == (int)warp.DoorType);
        }

        public Option<string> GetRequiredKey(Warp warp)
        {
            return warp.SomeWhen(x => (int)x.DoorType > 1)
                .FlatMap(w => _eifFileProvider.EIFFile
                    .SingleOrNone(item => item.Type == IO.ItemType.Key && item.Key == (int)w.DoorType)
                    .Map(x => x.Name));
        }
    }

    public interface IUnlockDoorValidator
    {
        bool CanMainCharacterOpenDoor(Warp warp);

        Option<string> GetRequiredKey(Warp warp);
    }
}