using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Services;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Board;
using EOLib.Domain.Login;
using EOLib.Graphics;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class BoardDialogFactory : IBoardDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IBoardActions _boardActions;
        private readonly IBoardRepository _boardRepository;
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IContentProvider _contentProvider;
        private readonly IHudControlProvider _hudControlProvider;

        public BoardDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                  IEODialogButtonService eoDialogButtonService,
                                  ILocalizedStringFinder localizedStringFinder,
                                  IEOMessageBoxFactory eoMessageBoxFactory,
                                  IBoardActions boardActions,
                                  IBoardRepository boardRepository,
                                  IPlayerInfoProvider playerInfoProvider,
                                  ICharacterProvider characterProvider,
                                  IContentProvider contentProvider,
                                  IHudControlProvider hudControlProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _eoDialogButtonService = eoDialogButtonService;
            _localizedStringFinder = localizedStringFinder;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _boardActions = boardActions;
            _boardRepository = boardRepository;
            _playerInfoProvider = playerInfoProvider;
            _characterProvider = characterProvider;
            _contentProvider = contentProvider;
            _hudControlProvider = hudControlProvider;
        }

        public BoardDialog Create()
        {
            return new BoardDialog(_nativeGraphicsManager,
                                   _eoDialogButtonService,
                                   _localizedStringFinder,
                                   _eoMessageBoxFactory,
                                   _boardActions,
                                   _boardRepository,
                                   _playerInfoProvider,
                                   _characterProvider,
                                   _contentProvider,
                                   _hudControlProvider);
        }
    }

    public interface IBoardDialogFactory
    {
        BoardDialog Create();
    }
}