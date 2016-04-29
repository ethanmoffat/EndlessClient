// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Net.Sockets;
using EndlessClient.Dialogs.Factories;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Protocol;
using EOLib.Net.Communication;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
	//todo: some of this should be split into services for getting display strings
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
					_messageBoxFactory.CreateMessageBox(DATCONST1.CONNECTION_SERVER_NOT_FOUND,
						XNADialogButtons.Ok,
						EOMessageBoxStyle.SmallDialogLargeHeader);
					break;
				default:
				{
					var errorCode = (int) connectResult;
					var ex = new SocketException(errorCode);
					_messageBoxFactory.CreateMessageBox(
						string.Format("Error code from socket: {0}", ex.SocketErrorCode),
						"Internal Error");
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
					var extra = string.Format(" 0.000.0{0}", versionNumber);
					_messageBoxFactory.CreateMessageBox(DATCONST1.CONNECTION_CLIENT_OUT_OF_DATE,
						extra,
						XNADialogButtons.Ok,
						EOMessageBoxStyle.SmallDialogLargeHeader);
				}
					break;
				case InitReply.BannedFromServer:
				{
					var banType = (BanType) initializationData[InitializationDataKey.BanType];
					if (banType == BanType.PermanentBan)
						_messageBoxFactory.CreateMessageBox(DATCONST1.CONNECTION_IP_BAN_PERM,
							XNADialogButtons.Ok,
							EOMessageBoxStyle.SmallDialogLargeHeader);
					else if (banType == BanType.TemporaryBan)
					{
						var banMinutesRemaining = initializationData[InitializationDataKey.BanTimeRemaining];
						var extra = string.Format(" {0} minutes.", banMinutesRemaining);
						_messageBoxFactory.CreateMessageBox(DATCONST1.CONNECTION_IP_BAN_TEMP,
							extra,
							XNADialogButtons.Ok,
							EOMessageBoxStyle.SmallDialogLargeHeader);
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
			_messageBoxFactory.CreateMessageBox(DATCONST1.CONNECTION_SERVER_NOT_FOUND,
				"\n\"" + ex.Message + "\"",
				XNADialogButtons.Ok,
				EOMessageBoxStyle.SmallDialogLargeHeader);
		}

		public void ShowException(EmptyPacketReceivedException ex)
		{
			_messageBoxFactory.CreateMessageBox(DATCONST1.CONNECTION_SERVER_NOT_FOUND,
				"\n\"" + ex.Message + "\"",
				XNADialogButtons.Ok,
				EOMessageBoxStyle.SmallDialogLargeHeader);
		}

		public void ShowLoginError(LoginReply loginError)
		{
			DATCONST1 message;
			switch (loginError)
			{
				case LoginReply.WrongUser: message = DATCONST1.LOGIN_ACCOUNT_NAME_NOT_FOUND; break;
				case LoginReply.WrongUserPass: message = DATCONST1.LOGIN_ACCOUNT_NAME_OR_PASSWORD_NOT_FOUND; break;
				case LoginReply.LoggedIn: message = DATCONST1.LOGIN_ACCOUNT_ALREADY_LOGGED_ON; break;
				case LoginReply.Busy: message = DATCONST1.CONNECTION_SERVER_IS_FULL;  break;
				default: throw new ArgumentOutOfRangeException("loginError", loginError, null);
			}

			_messageBoxFactory.CreateMessageBox(message,
				XNADialogButtons.Ok,
				EOMessageBoxStyle.SmallDialogLargeHeader);
		}

		public void ShowCharacterManagementMessage(CharacterReply characterError)
		{
			DATCONST1 message;
			switch (characterError)
			{
				case CharacterReply.Exists: message = DATCONST1.CHARACTER_CREATE_NAME_EXISTS; break;
				case CharacterReply.Full: message = DATCONST1.CHARACTER_CREATE_TOO_MANY_CHARS; break;
				case CharacterReply.NotApproved: message = DATCONST1.CHARACTER_CREATE_NAME_NOT_APPROVED; break;
				case CharacterReply.Ok: message = DATCONST1.CHARACTER_CREATE_SUCCESS; break;
				default: throw new ArgumentOutOfRangeException("characterError", characterError, null);
			}

			_messageBoxFactory.CreateMessageBox(message,
				XNADialogButtons.Ok,
				EOMessageBoxStyle.SmallDialogLargeHeader);
		}
	}
}
