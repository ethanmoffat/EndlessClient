// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Dialogs.Factories;
using EOLib;
using EOLib.Data.Protocol;
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
				default: throw new ArgumentOutOfRangeException();
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
	}
}
