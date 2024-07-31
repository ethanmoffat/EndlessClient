using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EOLib.Graphics;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [MappedType(BaseType = typeof(IGameLoadingDialogFactory))]
    public class GameLoadingDialogFactory : IGameLoadingDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public GameLoadingDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                        IGameStateProvider gameStateProvider,
                                        IClientWindowSizeProvider clientWindowSizeProvider,
                                        ILocalizedStringFinder localizedStringFinder)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _localizedStringFinder = localizedStringFinder;
        }

        public GameLoadingDialog CreateGameLoadingDialog()
        {
            return new GameLoadingDialog(_nativeGraphicsManager,
                _gameStateProvider,
                _clientWindowSizeProvider,
                _localizedStringFinder);
        }
    }

    public interface IGameLoadingDialogFactory
    {
        GameLoadingDialog CreateGameLoadingDialog();
    }
}