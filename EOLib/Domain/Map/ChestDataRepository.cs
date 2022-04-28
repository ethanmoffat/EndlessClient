using AutomaticTypeMapper;
using EOLib.Domain.Character;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    public interface IChestDataRepository : IResettable
    {
        MapCoordinate Location { get; set; }

        HashSet<ChestItem> Items { get; set; }
    }

    public interface IChestDataProvider : IResettable
    {
        MapCoordinate Location { get; }

        IReadOnlyCollection<ChestItem> Items { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ChestDataRepository : IChestDataProvider, IChestDataRepository
    {
        public MapCoordinate Location { get; set; }

        public HashSet<ChestItem> Items { get; set; }

        IReadOnlyCollection<ChestItem> IChestDataProvider.Items => Items;

        public ChestDataRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            Location = MapCoordinate.Zero;
            Items = new HashSet<ChestItem>();
        }
    }
}
