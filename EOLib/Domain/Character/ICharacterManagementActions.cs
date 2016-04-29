// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;

namespace EOLib.Domain.Character
{
	public interface ICharacterManagementActions
	{
		Task<CharacterReply> RequestCharacterCreation();

		Task<CharacterReply> CreateCharacter(ICharacterCreateParameters parameters);
		
		Task<int> RequestCharacterDelete();
		
		Task<CharacterReply> DeleteCharacter();
	}
}
