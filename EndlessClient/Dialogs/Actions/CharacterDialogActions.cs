// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Character;
using EOLib.Localization;
using XNAControls.Old;

namespace EndlessClient.Dialogs.Actions
{
    public class CharacterDialogActions : ICharacterDialogActions
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ICreateCharacterDialogFactory _createCharacterDialogFactory;

        public CharacterDialogActions(IEOMessageBoxFactory messageBoxFactory,
                                      ICreateCharacterDialogFactory createCharacterDialogFactory)
        {
            _messageBoxFactory = messageBoxFactory;
            _createCharacterDialogFactory = createCharacterDialogFactory;
        }

        public async Task<ICharacterCreateParameters> ShowCreateCharacterDialog()
        {
            try
            {
                var dialog = _createCharacterDialogFactory.BuildCreateCharacterDialog();
                return await dialog.Show();
            }
            catch (OperationCanceledException) { return null; }
        }

        public void ShowCharacterReplyDialog(CharacterReply response)
        {
            DialogResourceID message;
            switch (response)
            {
                case CharacterReply.Exists: message = DialogResourceID.CHARACTER_CREATE_NAME_EXISTS; break;
                case CharacterReply.Full: message = DialogResourceID.CHARACTER_CREATE_TOO_MANY_CHARS; break;
                case CharacterReply.NotApproved: message = DialogResourceID.CHARACTER_CREATE_NAME_NOT_APPROVED; break;
                case CharacterReply.Ok: message = DialogResourceID.CHARACTER_CREATE_SUCCESS; break;
                default: throw new ArgumentOutOfRangeException("response", response, null);
            }

            _messageBoxFactory.CreateMessageBox(message,
                XNADialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
        }

        public void ShowCharacterDeleteWarning(string characterName)
        {
            _messageBoxFactory.CreateMessageBox(
                string.Format("Character \'{0}\' ", characterName),
                DialogResourceID.CHARACTER_DELETE_FIRST_CHECK);
        }

        public async Task<XNADialogResult> ShowConfirmDeleteWarning(string characterName)
        {
            var messageBox = _messageBoxFactory.CreateMessageBox(
                string.Format("Character \'{0}\' ", characterName),
                DialogResourceID.CHARACTER_DELETE_CONFIRM,
                XNADialogButtons.OkCancel,
                EOMessageBoxStyle.SmallDialogLargeHeader);

            return await messageBox.Show();
        }

        public void ShowCharacterDeleteError()
        {
            _messageBoxFactory.CreateMessageBox(
                "The server did not respond properly for deleting the character. Try again.",
                "Server error",
                XNADialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
        }
    }
}