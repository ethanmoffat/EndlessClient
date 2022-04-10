using AutomaticTypeMapper;
using EOLib.Domain.Character;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    public interface ILockerDataRepository : IResettable
    {
        MapCoordinate Location { get; set; }

        HashSet<IInventoryItem> Items { get; set; }
    }

    public interface ILockerDataProvider : IResettable
    {
        MapCoordinate Location { get; }

        IReadOnlyCollection<IInventoryItem> Items { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class LockerDataRepository : ILockerDataProvider, ILockerDataRepository
    {
        public MapCoordinate Location { get; set; }

        public HashSet<IInventoryItem> Items { get; set; }

        IReadOnlyCollection<IInventoryItem> ILockerDataProvider.Items => Items;

        public LockerDataRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            Location = MapCoordinate.Zero;
            Items = new HashSet<IInventoryItem>();
        }
    }
}
