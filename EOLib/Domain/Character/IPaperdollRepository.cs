// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Character
{
    public interface IPaperdollRepository
    {
        List<short> ActiveCharacterPaperdoll { get; set; }

        Dictionary<int, List<short>> VisibleCharacterPaperdolls { get; set; }
    }

    public interface IPaperdollProvider
    {
        IReadOnlyList<short> ActiveCharacterPaperdoll { get; }

        IReadOnlyDictionary<int, IReadOnlyList<short>> VisibleCharacterPaperdolls { get; }
    }

    public class PaperdollRepository : IPaperdollRepository, IPaperdollProvider
    {
        public List<short> ActiveCharacterPaperdoll { get; set; }

        public Dictionary<int, List<short>> VisibleCharacterPaperdolls { get; set; }

        IReadOnlyList<short> IPaperdollProvider.ActiveCharacterPaperdoll
        {
            get { return ActiveCharacterPaperdoll; }
        }

        IReadOnlyDictionary<int, IReadOnlyList<short>> IPaperdollProvider.VisibleCharacterPaperdolls
        {
            get
            {
                return VisibleCharacterPaperdolls.ToDictionary(
                    k => k.Key,
                    v => (IReadOnlyList<short>)v.Value);
            }
        }
    }
}
