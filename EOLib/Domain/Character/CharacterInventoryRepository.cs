using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Character
{
    public interface ICharacterInventoryRepository
    {
        HashSet<IInventoryItem> ItemInventory { get; set; }

        HashSet<IInventorySpell> SpellInventory { get; set; }
    }

    public interface ICharacterInventoryProvider
    {
        IReadOnlyCollection<IInventoryItem> ItemInventory { get; }

        IReadOnlyCollection<IInventorySpell> SpellInventory { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CharacterInventoryRepository : ICharacterInventoryRepository, ICharacterInventoryProvider
    {
        public HashSet<IInventoryItem> ItemInventory { get; set; }
        public HashSet<IInventorySpell> SpellInventory { get; set; }

        IReadOnlyCollection<IInventoryItem> ICharacterInventoryProvider.ItemInventory => ItemInventory;
        IReadOnlyCollection<IInventorySpell> ICharacterInventoryProvider.SpellInventory => SpellInventory;

        public CharacterInventoryRepository()
        {
            ItemInventory = new HashSet<IInventoryItem>();
            SpellInventory = new HashSet<IInventorySpell>();
        }
    }
}
