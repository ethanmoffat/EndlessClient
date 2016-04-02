// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Data.Protocol;
using EOLib.Net.Communication;

namespace EndlessClient.Dialogs.Actions
{
	public class ErrorDialogDisplayAction : IErrorDialogDisplayAction
	{
		public void ShowError(ConnectResult connectResult)
		{
			switch (connectResult)
			{
				case ConnectResult.Timeout:
				case ConnectResult.InvalidEndpoint:
				case ConnectResult.InvalidSocket:
				case ConnectResult.SocketError:
					EOMessageBox.Show(DATCONST1.CONNECTION_SERVER_NOT_FOUND);
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
					EOMessageBox.Show(DATCONST1.CONNECTION_CLIENT_OUT_OF_DATE, extra);
				}
					break;
				case InitReply.BannedFromServer:
				{
					var banType = (BanType) initializationData[InitializationDataKey.BanType];
					if (banType == BanType.PermanentBan)
						EOMessageBox.Show(DATCONST1.CONNECTION_IP_BAN_PERM);
					else if (banType == BanType.TemporaryBan)
					{
						var banMinutesRemaining = initializationData[InitializationDataKey.BanTimeRemaining];
						var extra = string.Format(" {0} minutes.", banMinutesRemaining);
						EOMessageBox.Show(DATCONST1.CONNECTION_IP_BAN_TEMP, extra);
					}
				}
					break;
				case InitReply.ErrorState:
					ShowError(ConnectResult.SocketError);
					break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}
}
