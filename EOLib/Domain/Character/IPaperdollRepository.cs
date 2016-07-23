// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Character
{
    public interface IPaperdollRepository
    {
        List<EquippedItem> ActiveCharacterPaperdoll { get; set; }

        Dictionary<int, List<EquippedItem>> VisibleCharacterPaperdolls { get; set; }
    }

    public interface IPaperdollProvider
    {
        IReadOnlyList<EquippedItem> ActiveCharacterPaperdoll { get; }

        IReadOnlyDictionary<int, IReadOnlyList<EquippedItem>> VisibleCharacterPaperdolls { get; }
    }

    public class PaperdollRepository : IPaperdollRepository, IPaperdollProvider
    {
        public List<EquippedItem> ActiveCharacterPaperdoll { get; set; }

        public Dictionary<int, List<EquippedItem>> VisibleCharacterPaperdolls { get; set; }

        IReadOnlyList<EquippedItem> IPaperdollProvider.ActiveCharacterPaperdoll
        {
            get { return ActiveCharacterPaperdoll; }
        }

        IReadOnlyDictionary<int, IReadOnlyList<EquippedItem>> IPaperdollProvider.VisibleCharacterPaperdolls
        {
            get
            {
                return VisibleCharacterPaperdolls.ToDictionary(
                    k => k.Key,
                    v => (IReadOnlyList<EquippedItem>) v.Value);
            }
        }
    }
}
