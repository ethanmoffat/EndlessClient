// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EOLib;
using EOLib.Graphics;
using EOLib.IO.Services;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    public class CreateAccountProgressDialogFactory : ICreateAccountProgressDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
        private readonly ILocalizedStringService _localizedStringService;

        public CreateAccountProgressDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                                  IGameStateProvider gameStateProvider,
                                                  IGraphicsDeviceProvider graphicsDeviceProvider,
                                                  ILocalizedStringService localizedStringService)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _graphicsDeviceProvider = graphicsDeviceProvider;
            _localizedStringService = localizedStringService;
        }

        public ProgressDialog BuildCreateAccountProgressDialog()
        {
            var message = _localizedStringService.GetString(DialogResourceID.ACCOUNT_CREATE_ACCEPTED + 1);
            var caption = _localizedStringService.GetString(DialogResourceID.ACCOUNT_CREATE_ACCEPTED);
            
            return new ProgressDialog(_nativeGraphicsManager,
                                      _gameStateProvider,
                                      _graphicsDeviceProvider,
                                      message, caption);
        }
    }
}