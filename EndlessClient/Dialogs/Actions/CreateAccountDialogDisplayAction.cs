// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EOLib;
using EOLib.Graphics;
using EOLib.IO.Services;

namespace EndlessClient.Dialogs.Actions
{
	public class CreateAccountDialogDisplayAction : ICreateAccountDialogDisplayAction
	{
		private readonly ILocalizedStringService _localizedStringService;
		private readonly INativeGraphicsManager _nativeGraphicsManager;
		private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
		private readonly IGameStateProvider _gameStateProvider;

		public CreateAccountDialogDisplayAction(ILocalizedStringService localizedStringService,
												INativeGraphicsManager nativeGraphicsManager,
												IGraphicsDeviceProvider graphicsDeviceProvider,
												IGameStateProvider gameStateProvider)
		{
			_localizedStringService = localizedStringService;
			_nativeGraphicsManager = nativeGraphicsManager;
			_graphicsDeviceProvider = graphicsDeviceProvider;
			_gameStateProvider = gameStateProvider;
		}

		public void ShowCreateAccountDialog()
		{
			var createAccountDlg = new ScrollingMessageDialog(_nativeGraphicsManager, _graphicsDeviceProvider, _gameStateProvider);
			createAccountDlg.MessageText = string.Format("{0}\n\n{1}\n\n{2}",
				_localizedStringService.GetString(DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_1),
				_localizedStringService.GetString(DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_2),
				_localizedStringService.GetString(DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_3));
		}
	}
}