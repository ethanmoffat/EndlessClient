using EOLib.Net;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EndlessClient.Dialogs.Actions;

public interface IErrorDialogDisplayAction
{
    void ShowError(ConnectResult connectResult);

    void ShowError(InitReply replyCode, InitInitServerPacket.IReplyCodeData initializationData);

    void ShowException(NoDataSentException ex);

    void ShowException(EmptyPacketReceivedException ex);

    void ShowLoginError(LoginReply loginError);

    void ShowConnectionLost(bool isIngame);
}