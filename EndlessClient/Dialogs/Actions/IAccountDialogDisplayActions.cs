using EOLib.Domain.Account;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System.Threading.Tasks;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    public interface IAccountDialogDisplayActions
    {
        void ShowInitialCreateWarningDialog();

        Task<XNADialogResult> ShowCreatePendingDialog();

        Task<Option<IChangePasswordParameters>> ShowChangePasswordDialog();

        void ShowCreateParameterValidationError(CreateAccountParameterResult validationResult);
        
        void ShowCreateAccountServerError(AccountReply serverError);
        
        void ShowSuccessMessage();
    }
}
