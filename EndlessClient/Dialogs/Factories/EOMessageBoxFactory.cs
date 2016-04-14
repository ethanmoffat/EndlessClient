// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EOLib;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using XNAControls;

namespace EndlessClient.Dialogs.Factories
{
	public class EOMessageBoxFactory : IEOMessageBoxFactory
	{
		private readonly INativeGraphicsManager _nativeGraphicsManager;
		private readonly IGameStateProvider _gameStateProvider;
		private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
		private readonly IConfigurationProvider _configProvider;
		private readonly IDataFileProvider _dataFileProvider;

		public EOMessageBoxFactory(INativeGraphicsManager nativeGraphicsManager,
								   IGameStateProvider gameStateProvider,
								   IGraphicsDeviceProvider graphicsDeviceProvider,
								   IConfigurationProvider configProvider,
								   IDataFileProvider dataFileProvider)
		{
			_nativeGraphicsManager = nativeGraphicsManager;
			_gameStateProvider = gameStateProvider;
			_graphicsDeviceProvider = graphicsDeviceProvider;
			_configProvider = configProvider;
			_dataFileProvider = dataFileProvider;
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
			var file = _dataFileProvider.DataFiles[LocalizedFile];
			CreateMessageBox(file.Data[(int)resource + 1],
				file.Data[(int)resource],
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
			var file = _dataFileProvider.DataFiles[LocalizedFile];
			var message = prependData + file.Data[(int)resource + 1];
			CreateMessageBox(message,
				file.Data[(int)resource],
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
			var file = _dataFileProvider.DataFiles[LocalizedFile];
			var message = file.Data[(int)resource + 1] + extraData;
			CreateMessageBox(message,
				file.Data[(int)resource],
				whichButtons,
				style,
				closingEvent);
		}

		private DataFiles LocalizedFile
		{
			get
			{
				switch (_configProvider.Language)
				{
					case EOLanguage.Dutch: return DataFiles.DutchStatus1;
					case EOLanguage.Swedish: return DataFiles.SwedishStatus1;
					case EOLanguage.Portuguese: return DataFiles.PortugueseStatus1;
					default: return DataFiles.EnglishStatus1;
				}
			}
		}
	}
}