// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.Domain.Character
{
    public interface ICharacterRepository
    {
        ICharacter ActiveCharacter { get; set; }
    }

    public interface ICharacterProvider
    {
        ICharacter ActiveCharacter { get; }
    }

    public class CharacterRepository : ICharacterRepository, ICharacterProvider
    {
        public ICharacter ActiveCharacter { get; set; }

        public List<ICharacter> VisibleCharacters { get; set; }

        public CharacterRepository()
        {
            VisibleCharacters = new List<ICharacter>(64);
        }
    }
}
