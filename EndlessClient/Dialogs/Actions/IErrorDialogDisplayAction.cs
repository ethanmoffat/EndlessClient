// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Data.Login;
using EOLib.Data.Protocol;
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
	}
}
