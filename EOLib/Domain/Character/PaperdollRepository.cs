using AutomaticTypeMapper;
using System.Collections.Generic;

namespace EOLib.Domain.Character
{
    public interface IPaperdollRepository
    {
        Dictionary<int, PaperdollData> VisibleCharacterPaperdolls { get; set; }
    }

    public interface IPaperdollProvider
    {
        IReadOnlyDictionary<int, PaperdollData> VisibleCharacterPaperdolls { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class PaperdollRepository : IPaperdollRepository, IPaperdollProvider
    {
        public Dictionary<int, PaperdollData> VisibleCharacterPaperdolls { get; set; } = new Dictionary<int, PaperdollData>();

        IReadOnlyDictionary<int, PaperdollData> IPaperdollProvider.VisibleCharacterPaperdolls => VisibleCharacterPaperdolls;
    }
}