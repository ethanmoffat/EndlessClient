// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EOLib.Domain.Login;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    [MappedType(BaseType = typeof(IChangePasswordDialogFactory))]
    public class ChangePasswordDialogFactory : IChangePasswordDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IContentManagerProvider _contentManagerProvider;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly IEODialogButtonService _eoDialogButtonService;

        public ChangePasswordDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                           IGameStateProvider gameStateProvider,
                                           IContentManagerProvider contentManagerProvider,
                                           IEOMessageBoxFactory eoMessageBoxFactory,
                                           IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                           IPlayerInfoProvider playerInfoProvider,
                                           IEODialogButtonService eoDialogButtonService)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _contentManagerProvider = contentManagerProvider;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _keyboardDispatcherProvider = keyboardDispatcherProvider;
            _playerInfoProvider = playerInfoProvider;
            _eoDialogButtonService = eoDialogButtonService;
        }

        public ChangePasswordDialog BuildChangePasswordDialog()
        {
            return new ChangePasswordDialog(_nativeGraphicsManager,
                                            _gameStateProvider,
                                            _contentManagerProvider,
                                            _eoMessageBoxFactory,
                                            _keyboardDispatcherProvider,
                                            _playerInfoProvider,
                                            _eoDialogButtonService);
        }
    }
}