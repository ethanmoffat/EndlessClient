// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EOLib.Graphics;
using EOLib.IO.Services;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    public class GameLoadingDialogFactory : IGameLoadingDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public GameLoadingDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                        IGameStateProvider gameStateProvider,
                                        IGraphicsDeviceProvider graphicsDeviceProvider,
                                        IClientWindowSizeProvider clientWindowSizeProvider,
                                        ILocalizedStringFinder localizedStringFinder)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _graphicsDeviceProvider = graphicsDeviceProvider;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _localizedStringFinder = localizedStringFinder;
        }

        public GameLoadingDialog CreateGameLoadingDialog()
        {
            return new GameLoadingDialog(_nativeGraphicsManager,
                _gameStateProvider,
                _graphicsDeviceProvider,
                _clientWindowSizeProvider,
                _localizedStringFinder);
        }
    }
}