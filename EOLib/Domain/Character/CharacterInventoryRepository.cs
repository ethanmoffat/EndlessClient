// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Character
{
    public interface ICharacterInventoryRepository
    {
        List<IInventoryItem> ItemInventory { get; set; }

        List<IInventorySpell> SpellInventory { get; set; }
    }

    public interface ICharacterInventoryProvider
    {
        IReadOnlyList<IInventoryItem> ItemInventory { get; }

        IReadOnlyList<IInventorySpell> SpellInventory { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CharacterInventoryRepository : ICharacterInventoryRepository, ICharacterInventoryProvider
    {
        public List<IInventoryItem> ItemInventory { get; set; }
        public List<IInventorySpell> SpellInventory { get; set; }

        IReadOnlyList<IInventoryItem> ICharacterInventoryProvider.ItemInventory => ItemInventory;
        IReadOnlyList<IInventorySpell> ICharacterInventoryProvider.SpellInventory => SpellInventory;

        public CharacterInventoryRepository()
        {
            ItemInventory = new List<IInventoryItem>(32);
            SpellInventory = new List<IInventorySpell>(32);
        }
    }
}
