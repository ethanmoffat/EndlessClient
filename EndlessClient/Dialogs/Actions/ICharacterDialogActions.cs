using System.Threading.Tasks;
using EOLib;
using EOLib.Domain.Character;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    public interface ICharacterDialogActions
    {
        Task<Optional<ICharacterCreateParameters>> ShowCreateCharacterDialog();

        void ShowCharacterReplyDialog(CharacterReply response);

        void ShowCharacterDeleteWarning(string characterName);

        Task<XNADialogResult> ShowConfirmDeleteWarning(string characterName);

        void ShowCharacterDeleteError();
    }
}
