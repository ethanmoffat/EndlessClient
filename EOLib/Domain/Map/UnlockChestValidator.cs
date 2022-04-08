using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Optional;

namespace EOLib.Domain.Map
{
    [AutoMappedType]
    public class UnlockChestValidator : IUnlockChestValidator
    {
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEIFFileProvider _eifFileProvider;

        public UnlockChestValidator(ICharacterInventoryProvider characterInventoryProvider,
                                    IEIFFileProvider eifFileProvider)
        {
            _characterInventoryProvider = characterInventoryProvider;
            _eifFileProvider = eifFileProvider;
        }

        public bool CanMainCharacterOpenChest(ChestKey requiredKey)
        {
            return GetRequiredKeyName(requiredKey).Match(
                some: keyName => _characterInventoryProvider
                    .ItemInventory
                    .Where(x => _eifFileProvider.EIFFile[x.ItemID].Type == IO.ItemType.Key)
                    .Select(x => _eifFileProvider.EIFFile[x.ItemID].Name)
                    .Any(keyName.Equals),
                none: () => true);
        }

        public Option<string> GetRequiredKeyName(ChestKey requiredKey)
        {
            switch (requiredKey)
            {
                case ChestKey.Normal: return Option.Some("Normal Key");
                case ChestKey.Silver: return Option.Some("Silver Key");
                case ChestKey.Crystal: return Option.Some("Crystal Key");
                case ChestKey.Wraith: return Option.Some("Wraith Key");
                default: return Option.None<string>();
            }
        }
    }

    public interface IUnlockChestValidator
    {
        bool CanMainCharacterOpenChest(ChestKey requiredKey);

        Option<string> GetRequiredKeyName(ChestKey requiredKey);
    }
}
