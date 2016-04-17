// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EOLib;
using EOLib.Graphics;
using EOLib.IO.Services;
using XNAControls;

namespace EndlessClient.Dialogs.Factories
{
	public class EOMessageBoxFactory : IEOMessageBoxFactory
	{
		private readonly INativeGraphicsManager _nativeGraphicsManager;
		private readonly IGameStateProvider _gameStateProvider;
		private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
		private readonly ILocalizedStringService _localizedStringService;

		public EOMessageBoxFactory(INativeGraphicsManager nativeGraphicsManager,
								   IGameStateProvider gameStateProvider,
								   IGraphicsDeviceProvider graphicsDeviceProvider,
								   ILocalizedStringService localizedStringService)
		{
			_nativeGraphicsManager = nativeGraphicsManager;
			_gameStateProvider = gameStateProvider;
			_graphicsDeviceProvider = graphicsDeviceProvider;
			_localizedStringService = localizedStringService;
		}

		public void CreateMessageBox(string message,
									 string caption = "",
									 XNADialogButtons whichButtons = XNADialogButtons.Ok,
									 EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader,
									 XNADialog.OnDialogClose closeEvent = null)
		{
			var messageBox = new EOMessageBox(_nativeGraphicsManager,
				_gameStateProvider,
				_graphicsDeviceProvider,
				message,
				caption,
				style,
				whichButtons);
			if (closeEvent != null)
				messageBox.DialogClosing += closeEvent;
		}

		public void CreateMessageBox(DATCONST1 resource,
									 XNADialogButtons whichButtons = XNADialogButtons.Ok,
									 EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader,
									 XNADialog.OnDialogClose closingEvent = null)
		{
			CreateMessageBox(_localizedStringService.GetString(resource + 1),
				_localizedStringService.GetString(resource),
				whichButtons,
				style,
				closingEvent);
		}

		public void CreateMessageBox(string prependData,
									 DATCONST1 resource,
									 XNADialogButtons whichButtons = XNADialogButtons.Ok,
									 EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader,
									 XNADialog.OnDialogClose closingEvent = null)
		{
			var message = prependData + _localizedStringService.GetString(resource + 1);
			CreateMessageBox(message,
				_localizedStringService.GetString(resource),
				whichButtons,
				style,
				closingEvent);
		}

		public void CreateMessageBox(DATCONST1 resource,
									 string extraData,
									 XNADialogButtons whichButtons = XNADialogButtons.Ok,
									 EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader,
									 XNADialog.OnDialogClose closingEvent = null)
		{
			var message = _localizedStringService.GetString(resource + 1) + extraData;
			CreateMessageBox(message,
				_localizedStringService.GetString(resource),
				whichButtons,
				style,
				closingEvent);
		}
	}
}