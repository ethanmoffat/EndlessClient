using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.UIControls;
using EOLib.Domain.Login;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    [MappedType(BaseType = typeof(IChangePasswordDialogFactory))]
    public class ChangePasswordDialogFactory : IChangePasswordDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IContentProvider _contentProvider;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly IXnaControlSoundMapper _xnaControlSoundMapper;

        public ChangePasswordDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                           IGameStateProvider gameStateProvider,
                                           IContentProvider contentProvider,
                                           IEOMessageBoxFactory eoMessageBoxFactory,
                                           IPlayerInfoProvider playerInfoProvider,
                                           IEODialogButtonService eoDialogButtonService,
                                           IXnaControlSoundMapper xnaControlSoundMapper)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _contentProvider = contentProvider;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _playerInfoProvider = playerInfoProvider;
            _eoDialogButtonService = eoDialogButtonService;
            _xnaControlSoundMapper = xnaControlSoundMapper;
        }

        public ChangePasswordDialog BuildChangePasswordDialog()
        {
            return new ChangePasswordDialog(_nativeGraphicsManager,
                                            _gameStateProvider,
                                            _contentProvider,
                                            _eoMessageBoxFactory,
                                            _playerInfoProvider,
                                            _eoDialogButtonService,
                                            _xnaControlSoundMapper);
        }
    }
}