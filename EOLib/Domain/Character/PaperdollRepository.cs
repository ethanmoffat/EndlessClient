using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;

namespace EOLib.Domain.Character
{
    public interface IPaperdollRepository
    {
        List<short> MainCharacterPaperdoll { get; set; }

        Dictionary<int, List<short>> VisibleCharacterPaperdolls { get; set; }
    }

    public interface IPaperdollProvider
    {
        IReadOnlyList<short> MainCharacterPaperdoll { get; }

        IReadOnlyDictionary<int, IReadOnlyList<short>> VisibleCharacterPaperdolls { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class PaperdollRepository : IPaperdollRepository, IPaperdollProvider
    {
        public List<short> MainCharacterPaperdoll { get; set; }

        public Dictionary<int, List<short>> VisibleCharacterPaperdolls { get; set; }

        IReadOnlyList<short> IPaperdollProvider.MainCharacterPaperdoll => MainCharacterPaperdoll;

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
