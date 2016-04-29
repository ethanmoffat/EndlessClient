// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Character;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
	public interface ICharacterDialogActions
	{
		Task<ICharacterCreateParameters> ShowCreateCharacterDialog();

		void ShowCharacterReplyDialog(CharacterReply response);

		void ShowCharacterDeleteWarning(string characterName);
		
		Task<XNADialogResult> ShowConfirmDeleteWarning();
	}
}
