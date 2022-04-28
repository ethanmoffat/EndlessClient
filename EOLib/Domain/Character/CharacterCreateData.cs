using System.Collections.Generic;
using EOLib.Net.Translators;

namespace EOLib.Domain.Character
{
    public class CharacterCreateData : ICharacterCreateData
    {
        public CharacterReply Response { get; }

        private readonly List<Character> _characters;
        public IReadOnlyList<Character> Characters => _characters;

        public CharacterCreateData(CharacterReply response, List<Character> characters)
        {
            Response = response;
            _characters = characters;
        }
    }

    public interface ICharacterCreateData : ITranslatedData
    {
        CharacterReply Response { get; }

        IReadOnlyList<Character> Characters { get; }
    }
}