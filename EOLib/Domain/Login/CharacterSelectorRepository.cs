using AutomaticTypeMapper;
using Optional;
using System.Collections.Generic;

namespace EOLib.Domain.Login
{
    public interface ICharacterSelectorRepository
    {
        IReadOnlyList<Character.Character> Characters { get; set; }

        Option<Character.Character> CharacterForDelete { get; set; }
    }

    public interface ICharacterSelectorProvider
    {
        IReadOnlyList<Character.Character> Characters { get; }

        Option<Character.Character> CharacterForDelete { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CharacterSelectorRepository : ICharacterSelectorRepository, ICharacterSelectorProvider
    {
        public IReadOnlyList<Character.Character> Characters { get; set; }

        public Option<Character.Character> CharacterForDelete { get; set; }
    }
}