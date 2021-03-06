﻿using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;

namespace EOLib.Domain.Login
{
    public interface ICharacterSelectorRepository
    {
        IReadOnlyList<ICharacter> Characters { get; set; }

        ICharacter CharacterForDelete { get; set; }
    }

    public interface ICharacterSelectorProvider
    {
        IReadOnlyList<ICharacter> Characters { get; }

        ICharacter CharacterForDelete { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CharacterSelectorRepository : ICharacterSelectorRepository, ICharacterSelectorProvider
    {
        public IReadOnlyList<ICharacter> Characters { get; set; }

        public ICharacter CharacterForDelete { get; set; }
    }
}
