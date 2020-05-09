using System.Collections.Generic;
using EOLib.Net.Translators;

namespace EOLib.Domain.Character
{
    public class CharacterCreateData : ICharacterCreateData
    {
        public CharacterReply Response { get; }

        private readonly List<ICharacter> _characters;
        public IReadOnlyList<ICharacter> Characters => _characters;

        public CharacterCreateData(CharacterReply response, List<ICharacter> characters)
        {
            Response = response;
            _characters = characters;
        }
    }

    public interface ICharacterCreateData : ITranslatedData
    {
        CharacterReply Response { get; }

        IReadOnlyList<ICharacter> Characters { get; }
    }
}