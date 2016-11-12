// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Net.Translators;

namespace EOLib.Domain.Character
{
    public class CharacterCreateData : ICharacterCreateData
    {
        public CharacterReply Response { get; private set; }

        private readonly List<ICharacter> _characters;
        public IReadOnlyList<ICharacter> Characters { get { return _characters; } }

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