using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Character
{
    public interface ICharacterInventoryRepository
    {
        HashSet<InventoryItem> ItemInventory { get; set; }

        HashSet<InventorySpell> SpellInventory { get; set; }
    }

    public interface ICharacterInventoryProvider
    {
        IReadOnlyCollection<InventoryItem> ItemInventory { get; }

        IReadOnlyCollection<InventorySpell> SpellInventory { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CharacterInventoryRepository : ICharacterInventoryRepository, ICharacterInventoryProvider
    {
        public HashSet<InventoryItem> ItemInventory { get; set; }
        public HashSet<InventorySpell> SpellInventory { get; set; }

        IReadOnlyCollection<InventoryItem> ICharacterInventoryProvider.ItemInventory => ItemInventory;
        IReadOnlyCollection<InventorySpell> ICharacterInventoryProvider.SpellInventory => SpellInventory;

        public CharacterInventoryRepository()
        {
            ItemInventory = new HashSet<InventoryItem>();
            SpellInventory = new HashSet<InventorySpell>();
        }
    }
}