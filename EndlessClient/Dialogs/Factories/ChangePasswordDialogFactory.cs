// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Content;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EOLib.Domain.Login;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    public class ChangePasswordDialogFactory : IChangePasswordDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IContentManagerProvider _contentManagerProvider;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public ChangePasswordDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                           IGraphicsDeviceProvider graphicsDeviceProvider,
                                           IGameStateProvider gameStateProvider,
                                           IContentManagerProvider contentManagerProvider,
                                           IEOMessageBoxFactory eoMessageBoxFactory,
                                           IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                           IPlayerInfoProvider playerInfoProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _graphicsDeviceProvider = graphicsDeviceProvider;
            _gameStateProvider = gameStateProvider;
            _contentManagerProvider = contentManagerProvider;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _keyboardDispatcherProvider = keyboardDispatcherProvider;
            _playerInfoProvider = playerInfoProvider;
        }

        public ChangePasswordDialog BuildChangePasswordDialog()
        {
            return new ChangePasswordDialog(_nativeGraphicsManager,
                _graphicsDeviceProvider,
                _gameStateProvider,
                _contentManagerProvider,
                _eoMessageBoxFactory,
                _keyboardDispatcherProvider,
                _playerInfoProvider);
        }
    }
}