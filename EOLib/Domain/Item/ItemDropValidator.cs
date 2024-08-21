using System;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Repositories;

namespace EOLib.Domain.Item
{
    [AutoMappedType]
    public class ItemDropValidator : IItemDropValidator
    {
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        public ItemDropValidator(IEIFFileProvider eifFileProvider,
                                 ICurrentMapStateProvider currentMapStateProvider)
        {
            _eifFileProvider = eifFileProvider;
            _currentMapStateProvider = currentMapStateProvider;
        }

        public ItemDropResult ValidateItemDrop(Character.Character mainCharacter, InventoryItem item)
        {
            var coord = new MapCoordinate(mainCharacter.RenderProperties.MapX, mainCharacter.RenderProperties.MapY);
            return ValidateItemDrop(mainCharacter, item, coord);
        }

        public ItemDropResult ValidateItemDrop(Character.Character mainCharacter, InventoryItem item, MapCoordinate dropPoint)
        {
            if (item.ItemID <= 0)
                throw new ArgumentException("Item ID is invalid", nameof(item));

            var itemData = _eifFileProvider.EIFFile[item.ItemID];

            if (itemData.Special == ItemSpecial.Lore)
                return ItemDropResult.Lore;

            if (_currentMapStateProvider.IsJail)
                return ItemDropResult.Jail;

            var rp = mainCharacter.RenderProperties;
            if (dropPoint != MapCoordinate.Max && Math.Max(Math.Abs(rp.MapX - dropPoint.X), Math.Abs(rp.MapY - dropPoint.Y)) > 2)
                return ItemDropResult.TooFar;

            return ItemDropResult.Ok;
        }
    }

    public interface IItemDropValidator
    {
        ItemDropResult ValidateItemDrop(Character.Character mainCharacter, InventoryItem item);

        ItemDropResult ValidateItemDrop(Character.Character mainCharacter, InventoryItem item, MapCoordinate dropPoint);
    }
}
