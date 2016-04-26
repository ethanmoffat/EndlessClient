// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Character
{
	public interface ICharacterCreateParameters
	{
		string Name { get; }

		int Gender { get; }
		int HairStyle { get; }
		int HairColor { get; }
		int Race { get; }
	}
}
