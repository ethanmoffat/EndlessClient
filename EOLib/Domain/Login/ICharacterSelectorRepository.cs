// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.BLL;

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

	public class CharacterSelectorRepository : ICharacterSelectorRepository, ICharacterSelectorProvider
	{
		public IReadOnlyList<ICharacter> Characters { get; set; }

		public ICharacter CharacterForDelete { get; set; }
	}
}
