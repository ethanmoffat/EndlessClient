using EOLib.Domain.Login;
using EOLib.Domain.Protocol;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EndlessClient.Dialogs.Actions
{
    public interface IErrorDialogDisplayAction
    {
        void ShowError(ConnectResult connectResult);

        void ShowError(IInitializationData initializationData);

        void ShowException(NoDataSentException ex);

        void ShowException(EmptyPacketReceivedException ex);

        void ShowLoginError(LoginReply loginError);

        void ShowConnectionLost(bool isIngame);
    }
}
