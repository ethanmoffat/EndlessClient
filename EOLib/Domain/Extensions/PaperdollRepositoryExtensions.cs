﻿using System.Collections.Generic;
using System.Linq;
using EOLib.Domain.Character;
using EOLib.IO.Pub;

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
            return repository.MainCharacterPaperdoll.Select(itemID => (IEquippedItem)new EquippedItem(eifFile[itemID])).ToList();
        }
    }
}
