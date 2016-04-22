// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EndlessClient.Dialogs.Factories;

namespace EndlessClient.Controllers
{
	public class CharacterManagementController : ICharacterManagementController
	{
		private readonly ICreateCharacterDialogFactory _createCharacterDialogFactory;

		public CharacterManagementController(ICreateCharacterDialogFactory createCharacterDialogFactory)
		{
			_createCharacterDialogFactory = createCharacterDialogFactory;
		}

		public async Task CreateCharacter()
		{
			//request creation
			//check reply
			//show dialog
			//create with parameters
			//show confirm dialog
			//show any error dialogs during process
			await Task.FromResult(1);
		}

		public Task RequestDeleteCharacter()
		{
			//requests delete (first click)
			return Task.FromResult(false);
		}

		public Task ConfirmDeleteCharacter()
		{
			//does actual delete (second click)
			return Task.FromResult(false);
		}
	}
}