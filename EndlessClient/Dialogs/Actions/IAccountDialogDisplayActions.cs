using System.Threading.Tasks;
using EOLib;
using EOLib.Domain.Account;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    public interface IAccountDialogDisplayActions
    {
        void ShowInitialCreateWarningDialog();

        Task<XNADialogResult> ShowCreatePendingDialog();

        Task<Optional<IChangePasswordParameters>> ShowChangePasswordDialog();

        void ShowCreateParameterValidationError(CreateAccountParameterResult validationResult);
        
        void ShowCreateAccountServerError(AccountReply serverError);
        
        void ShowSuccessMessage();
    }
}
