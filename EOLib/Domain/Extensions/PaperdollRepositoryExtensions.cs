// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EOLib.Domain.Character;
using EOLib.IO.Pub;
using EOLib.IO.Extensions;

namespace EOLib.Domain.Extensions
{
    public static class PaperdollRepositoryExtensions
    {
        public static IReadOnlyList<IEquippedItem> GetLoggedInCharacterPaperdoll(this IPaperdollRepository repository, IPubFile<EIFRecord> eifFile)
        {
            return GetLoggedInCharacterPaperdoll((PaperdollRepository)repository, eifFile);
        }

        public static IReadOnlyList<IEquippedItem> GetLoggedInCharacterPaperdoll(this IPaperdollProvider provider, IPubFile<EIFRecord> eifFile)
        {
            return GetLoggedInCharacterPaperdoll((PaperdollRepository)provider, eifFile);
        }

        private static IReadOnlyList<IEquippedItem> GetLoggedInCharacterPaperdoll(this PaperdollRepository repository, IPubFile<EIFRecord> eifFile)
        {
            return repository.ActiveCharacterPaperdoll.Select(
                itemID =>
                {
                    var itemData = eifFile[itemID];
                    return (IEquippedItem)new EquippedItem(itemData.GetEquipLocation(), itemID);
                }).ToList();
        }
    }
}
