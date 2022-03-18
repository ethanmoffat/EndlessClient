using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using Optional;

namespace EOLib.Domain.Login
{
    public interface ICharacterSelectorRepository
    {
        IReadOnlyList<ICharacter> Characters { get; set; }

        Option<ICharacter> CharacterForDelete { get; set; }
    }

    public interface ICharacterSelectorProvider
    {
        IReadOnlyList<ICharacter> Characters { get; }

        Option<ICharacter> CharacterForDelete { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CharacterSelectorRepository : ICharacterSelectorRepository, ICharacterSelectorProvider
    {
        public IReadOnlyList<ICharacter> Characters { get; set; }

        public Option<ICharacter> CharacterForDelete { get; set; }
    }
}
