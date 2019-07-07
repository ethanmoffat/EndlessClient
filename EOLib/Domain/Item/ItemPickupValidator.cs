// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;
using AutomaticTypeMapper;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.IO.Repositories;

namespace EOLib.Domain.Item
{
    [AutoMappedType]
    public class ItemPickupValidator : IItemPickupValidator
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IEIFFileProvider _eifFileProvider;

        public ItemPickupValidator(IConfigurationProvider configurationProvider,
                                   IEIFFileProvider eifFileProvider)
        {
            _configurationProvider = configurationProvider;
            _eifFileProvider = eifFileProvider;
        }

        public ItemPickupResult ValidateItemPickup(ICharacter mainCharacter, IItem item)
        {
            var now = DateTime.Now;

            var xDif = Math.Abs(item.X - mainCharacter.RenderProperties.MapX);
            var yDif = Math.Abs(item.Y - mainCharacter.RenderProperties.MapY);
            if (xDif > 2 || yDif > 2)
                return ItemPickupResult.TooFar;

            if (item.OwningPlayerID.HasValue && mainCharacter.ID != item.OwningPlayerID.Value && item.DropTime.HasValue)
            {
                var dropTime = item.DropTime.Value;
                if (item.IsNPCDrop && (now - dropTime).TotalSeconds <= _configurationProvider.NPCDropProtectTime ||
                    !item.IsNPCDrop && (now - dropTime).TotalSeconds <= _configurationProvider.PlayerDropProtectTime)
                    return ItemPickupResult.DropProtection;
            }
            else
            {
                var itemData = _eifFileProvider.EIFFile[item.ItemID];
                var totalWeight = itemData.Weight * item.Amount;
                if (totalWeight + mainCharacter.Stats[CharacterStat.Weight] > mainCharacter.Stats[CharacterStat.MaxWeight])
                    return ItemPickupResult.TooHeavy;
            }

            return ItemPickupResult.Ok;
        }
    }

    public interface IItemPickupValidator
    {
        ItemPickupResult ValidateItemPickup(ICharacter mainCharacter, IItem item);
    }
}
