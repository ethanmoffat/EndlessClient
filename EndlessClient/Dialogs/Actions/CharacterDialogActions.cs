using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Character;
using EOLib.Localization;
using Optional;
using System;
using System.Threading.Tasks;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    [MappedType(BaseType = typeof(ICharacterDialogActions))]
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

        public Task<Option<ICharacterCreateParameters>> ShowCreateCharacterDialog()
        {
            var dialog = _createCharacterDialogFactory.BuildCreateCharacterDialog();
            return dialog.ShowDialogAsync()
                .ContinueWith(dialogTask =>
                    dialogTask.Result.SomeWhen(x => x == XNADialogResult.OK)
                        .Map<ICharacterCreateParameters>(x => new CharacterCreateParameters(dialog.Name, dialog.Gender, dialog.HairStyle, dialog.HairColor, dialog.Race)));
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

        public Task<XNADialogResult> ShowConfirmDeleteWarning(string characterName)
        {
            var messageBox = _messageBoxFactory.CreateMessageBox(
                $"Character \'{characterName}\' ",
                DialogResourceID.CHARACTER_DELETE_CONFIRM,
                EODialogButtons.OkCancel,
                EOMessageBoxStyle.SmallDialogLargeHeader);

            return messageBox.ShowDialogAsync();
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
