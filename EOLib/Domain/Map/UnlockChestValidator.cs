using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Optional;
using Optional.Collections;
using System.Linq;

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
            return requiredKey == ChestKey.None ||
                _characterInventoryProvider
                    .ItemInventory
                    .Any(x => _eifFileProvider.EIFFile[x.ItemID].Type == IO.ItemType.Key &&
                              _eifFileProvider.EIFFile[x.ItemID].Key == (int)requiredKey);
        }

        public Option<string> GetRequiredKeyName(ChestKey requiredKey)
        {
            return _eifFileProvider.EIFFile.SingleOrNone(x => x.Type == IO.ItemType.Key && x.Key == (int)requiredKey).Map(x => x.Name);
        }
    }

    public interface IUnlockChestValidator
    {
        bool CanMainCharacterOpenChest(ChestKey requiredKey);

        Option<string> GetRequiredKeyName(ChestKey requiredKey);
    }
}