using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Factories;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class ErrorDialogDisplayAction : IErrorDialogDisplayAction
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ISfxPlayer _sfxPlayer;

        public ErrorDialogDisplayAction(IEOMessageBoxFactory messageBoxFactory,
                                        ISfxPlayer sfxPlayer)
        {
            _messageBoxFactory = messageBoxFactory;
            _sfxPlayer = sfxPlayer;
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

        public void ShowError(InitReply replyCode, InitInitServerPacket.IReplyCodeData initializationData)
        {
            switch (replyCode)
            {
                case InitReply.OutOfDate:
                    {
                        var data = (InitInitServerPacket.ReplyCodeDataOutOfDate)initializationData;
                        var extra = $" {data.Version.Major:D3}.{data.Version.Minor:D3}.{data.Version.Patch:D3}";
                        var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CONNECTION_CLIENT_OUT_OF_DATE,
                            extra,
                            EODialogButtons.Ok,
                            EOMessageBoxStyle.SmallDialogLargeHeader);
                        messageBox.ShowDialog();
                    }
                    break;
                case InitReply.Banned:
                    {
                        var data = (InitInitServerPacket.ReplyCodeDataBanned)initializationData;
                        if (data.BanType == InitBanType.Permanent)
                        {
                            var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CONNECTION_IP_BAN_PERM,
                                EODialogButtons.Ok,
                                EOMessageBoxStyle.SmallDialogLargeHeader);
                            messageBox.ShowDialog();
                        }
                        else if (data.BanType == InitBanType.Temporary || data.BanType == 0)
                        {
                            var banMinutesRemaining = data.BanTypeData is InitInitServerPacket.ReplyCodeDataBanned.BanTypeData0 dataZero
                                ? dataZero.MinutesRemaining
                                : data.BanTypeData is InitInitServerPacket.ReplyCodeDataBanned.BanTypeDataTemporary dataTemp
                                    ? dataTemp.MinutesRemaining
                                    : throw new ArgumentException();
                            var extra = $" {banMinutesRemaining} minutes.";
                            var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CONNECTION_IP_BAN_TEMP,
                                extra,
                                EODialogButtons.Ok,
                                EOMessageBoxStyle.SmallDialogLargeHeader);
                            messageBox.ShowDialog();
                        }

                        _sfxPlayer.PlaySfx(SoundEffectID.Banned);
                    }
                    break;
                case 0:
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
            var message = loginError switch
            {
                LoginReply.WrongUser => DialogResourceID.LOGIN_ACCOUNT_NAME_NOT_FOUND,
                LoginReply.WrongUserPassword => DialogResourceID.LOGIN_ACCOUNT_NAME_OR_PASSWORD_NOT_FOUND,
                LoginReply.Banned => DialogResourceID.LOGIN_BANNED_FROM_SERVER,
                LoginReply.LoggedIn => DialogResourceID.LOGIN_ACCOUNT_ALREADY_LOGGED_ON,
                LoginReply.Busy => DialogResourceID.CONNECTION_SERVER_IS_FULL,
                _ => DialogResourceID.LOGIN_SERVER_COULD_NOT_PROCESS,
            };
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
