using EOLib.Domain.Character;
using Optional;
using System.Threading.Tasks;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    public interface ICharacterDialogActions
    {
        Task<Option<ICharacterCreateParameters>> ShowCreateCharacterDialog();

        void ShowCharacterReplyDialog(CharacterReply response);

        void ShowCharacterDeleteWarning(string characterName);

        Task<XNADialogResult> ShowConfirmDeleteWarning(string characterName);

        void ShowCharacterDeleteError();
    }
}
