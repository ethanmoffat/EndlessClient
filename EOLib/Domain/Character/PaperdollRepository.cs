using AutomaticTypeMapper;
using System.Collections.Generic;

namespace EOLib.Domain.Character
{
    public interface IPaperdollRepository
    {
        Dictionary<int, IPaperdollData> VisibleCharacterPaperdolls { get; set; }
    }

    public interface IPaperdollProvider
    {
        IReadOnlyDictionary<int, IPaperdollData> VisibleCharacterPaperdolls { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class PaperdollRepository : IPaperdollRepository, IPaperdollProvider
    {
        public Dictionary<int, IPaperdollData> VisibleCharacterPaperdolls { get; set; } = new Dictionary<int, IPaperdollData>();

        IReadOnlyDictionary<int, IPaperdollData> IPaperdollProvider.VisibleCharacterPaperdolls => VisibleCharacterPaperdolls;
    }
}
