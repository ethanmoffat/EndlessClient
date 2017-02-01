// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Factories;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Localization;
using XNAControls;

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

        public async Task<Optional<ICharacterCreateParameters>> ShowCreateCharacterDialog()
        {
            var dialog = _createCharacterDialogFactory.BuildCreateCharacterDialog();
            var result = await dialog.ShowDialogAsync();
            return result == XNADialogResult.OK
                ? new CharacterCreateParameters(dialog.Name, dialog.Gender, dialog.HairStyle, dialog.HairColor, dialog.Race)
                : Optional<ICharacterCreateParameters>.Empty;
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
                default: throw new ArgumentOutOfRangeException(nameof(response), response, null);
            }

            var messageBox = _messageBoxFactory.CreateMessageBox(message,
                EODialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
            messageBox.ShowDialog();
        }

        public void ShowCharacterDeleteWarning(string characterName)
        {
            var messageBox = _messageBoxFactory.CreateMessageBox(
                $"Character \'{characterName}\' ",
                DialogResourceID.CHARACTER_DELETE_FIRST_CHECK);
            messageBox.ShowDialog();
        }

        public async Task<XNADialogResult> ShowConfirmDeleteWarning(string characterName)
        {
            var messageBox = _messageBoxFactory.CreateMessageBox(
                $"Character \'{characterName}\' ",
                DialogResourceID.CHARACTER_DELETE_CONFIRM,
                EODialogButtons.OkCancel,
                EOMessageBoxStyle.SmallDialogLargeHeader);

            return await messageBox.ShowDialogAsync();
        }

        public void ShowCharacterDeleteError()
        {
            var messageBox = _messageBoxFactory.CreateMessageBox(
                "The server did not respond properly for deleting the character. Try again.",
                "Server error",
                EODialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
            messageBox.ShowDialog();
        }
    }
}