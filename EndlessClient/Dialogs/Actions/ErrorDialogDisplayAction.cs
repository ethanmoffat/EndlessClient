using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Login;
using EOLib.Domain.Protocol;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EndlessClient.Dialogs.Actions
{
    //todo: some of this should be split into services for getting display strings
    [MappedType(BaseType = typeof(IErrorDialogDisplayAction))]
    public class ErrorDialogDisplayAction : IErrorDialogDisplayAction
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;

        public ErrorDialogDisplayAction(IEOMessageBoxFactory messageBoxFactory)
        {
            _messageBoxFactory = messageBoxFactory;
        }

        public void ShowError(ConnectResult connectResult)
        {
            switch (connectResult)
            {
                case ConnectResult.Timeout:
                case ConnectResult.InvalidEndpoint:
                case ConnectResult.InvalidSocket:
                case ConnectResult.SocketError:
                    {
                        var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CONNECTION_SERVER_NOT_FOUND,
                            EODialogButtons.Ok,
                            EOMessageBoxStyle.SmallDialogLargeHeader);
                        messageBox.ShowDialog();
                    }
                    break;
                default:
                    {
                        var errorCode = (int)connectResult;
                        var ex = new SocketException(errorCode);

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            if (Enum.TryParse<SocketError>(errorCode.ToString(), out var socketError))
                            {
                                switch (socketError)
                                {
                                    case SocketError.HostUnreachable:
                                    case SocketError.HostNotFound:
                                    case SocketError.HostDown:
                                        var hostDownMessageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CONNECTION_SERVER_NOT_FOUND,
                                            EODialogButtons.Ok,
                                            EOMessageBoxStyle.SmallDialogLargeHeader);
                                        hostDownMessageBox.ShowDialog();
                                        return;

                                    // For some reason, this socket error code does not exist in the SocketError enum, but does occurr when the host is unreachable.
                                    default:
                                        if (errorCode == 10063)
                                            goto case SocketError.HostDown;
                                        break;
                                }
                            }
                        }

                        var messageBox = _messageBoxFactory.CreateMessageBox(
                            $"Error code from socket: {ex.SocketErrorCode}",
                            "Internal Error");
                        messageBox.ShowDialog();
                    }
                    break;
            }
        }

        public void ShowError(IInitializationData initializationData)
        {
            switch (initializationData.Response)
            {
                case InitReply.ClientOutOfDate:
                    {
                        var versionNumber = initializationData[InitializationDataKey.RequiredVersionNumber];
                        var extra = $" 0.000.0{versionNumber}";
                        var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CONNECTION_CLIENT_OUT_OF_DATE,
                            extra,
                            EODialogButtons.Ok,
                            EOMessageBoxStyle.SmallDialogLargeHeader);
                        messageBox.ShowDialog();
                    }
                    break;
                case InitReply.BannedFromServer:
                    {
                        var banType = (BanType)initializationData[InitializationDataKey.BanType];
                        if (banType == BanType.PermanentBan)
                        {
                            var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CONNECTION_IP_BAN_PERM,
                                EODialogButtons.Ok,
                                EOMessageBoxStyle.SmallDialogLargeHeader);
                            messageBox.ShowDialog();
                        }
                        else if (banType == BanType.TemporaryBan)
                        {
                            var banMinutesRemaining = initializationData[InitializationDataKey.BanTimeRemaining];
                            var extra = $" {banMinutesRemaining} minutes.";
                            var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CONNECTION_IP_BAN_TEMP,
                                extra,
                                EODialogButtons.Ok,
                                EOMessageBoxStyle.SmallDialogLargeHeader);
                            messageBox.ShowDialog();
                        }
                    }
                    break;
                case InitReply.ErrorState:
                    ShowError(ConnectResult.SocketError);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void ShowException(NoDataSentException ex)
        {
            var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CONNECTION_SERVER_NOT_FOUND,
                "\n\"" + ex.Message + "\"",
                EODialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
            messageBox.ShowDialog();
        }

        public void ShowException(EmptyPacketReceivedException ex)
        {
            var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CONNECTION_SERVER_NOT_FOUND,
                "\n\"" + ex.Message + "\"",
                EODialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
            messageBox.ShowDialog();
        }

        public void ShowLoginError(LoginReply loginError)
        {
            DialogResourceID message;
            switch (loginError)
            {
                case LoginReply.WrongUser: message = DialogResourceID.LOGIN_ACCOUNT_NAME_NOT_FOUND; break;
                case LoginReply.WrongUserPass: message = DialogResourceID.LOGIN_ACCOUNT_NAME_OR_PASSWORD_NOT_FOUND; break;
                case LoginReply.AccountBanned: message = DialogResourceID.LOGIN_BANNED_FROM_SERVER; break;
                case LoginReply.LoggedIn: message = DialogResourceID.LOGIN_ACCOUNT_ALREADY_LOGGED_ON; break;
                case LoginReply.Busy: message = DialogResourceID.CONNECTION_SERVER_IS_FULL; break;
                case LoginReply.THIS_IS_WRONG: message = DialogResourceID.LOGIN_SERVER_COULD_NOT_PROCESS; break;
                default: throw new ArgumentOutOfRangeException(nameof(loginError), loginError, null);
            }

            var messageBox = _messageBoxFactory.CreateMessageBox(message,
                EODialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
            messageBox.ShowDialog();
        }

        public void ShowConnectionLost(bool isInGame)
        {
            var resource = isInGame ? DialogResourceID.CONNECTION_LOST_IN_GAME : DialogResourceID.CONNECTION_LOST_CONNECTION;
            var style = isInGame ? EOMessageBoxStyle.SmallDialogSmallHeader : EOMessageBoxStyle.SmallDialogLargeHeader;

            var messageBox = _messageBoxFactory.CreateMessageBox(resource, style: style);
            messageBox.ShowDialog();
        }
    }
}
