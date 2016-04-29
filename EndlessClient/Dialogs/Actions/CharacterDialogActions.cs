// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Dialogs.Factories;
using EOLib;

namespace EndlessClient.Dialogs.Actions
{
	public class CharacterDialogActions : ICharacterDialogActions
	{
		private readonly IEOMessageBoxFactory _messageBoxFactory;

		public CharacterDialogActions(IEOMessageBoxFactory messageBoxFactory)
		{
			_messageBoxFactory = messageBoxFactory;
		}

		public void ShowCharacterDeleteWarning(string characterName)
		{
			_messageBoxFactory.CreateMessageBox(
				string.Format("Character \'{0}\'", characterName),
				DATCONST1.CHARACTER_DELETE_FIRST_CHECK);
		}
	}
}