// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Character
{
    public interface ICharacterRepository
    {
        ICharacter MainCharacter { get; set; }
    }

    public interface ICharacterProvider
    {
        ICharacter MainCharacter { get; }
    }

    public class CharacterRepository : ICharacterRepository, ICharacterProvider
    {
        public ICharacter MainCharacter { get; set; }
    }
}
