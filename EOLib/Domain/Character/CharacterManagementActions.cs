// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Login;

namespace EOLib.Domain.Character
{
	public class CharacterManagementActions : ICharacterManagementActions
	{
		public async Task<CharacterReply> RequestCharacterCreation()
		{
			return await Task.FromResult(CharacterReply.THIS_IS_WRONG);
		}

		public async Task<ICharacterCreateData> CreateCharacter(ICharacterCreateParameters parameters)
		{
			return await Task.FromResult((ICharacterCreateData)null);
		}
	}
}