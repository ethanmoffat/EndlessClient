using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.Dialogs;
using EndlessClient.HUD;
using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;
using EOLib.Domain.Item;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.Factories
{
    [AutoMappedType]
    public class MouseCursorRendererFactory : IMouseCursorRendererFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly IItemStringService _itemStringService;
        private readonly IItemNameColorService _itemNameColorService;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IMapInteractionController _mapInteractionController;
        private readonly IUserInputProvider _userInputProvider;
        private readonly IActiveDialogProvider _activeDialogProvider;
        private readonly IContextMenuProvider _contextMenuProvider;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;

        public MouseCursorRendererFactory(INativeGraphicsManager nativeGraphicsManager,
                                          ICharacterProvider characterProvider,
                                          ICharacterRendererProvider characterRendererProvider,
                                          IRenderOffsetCalculator renderOffsetCalculator,
                                          IMapCellStateProvider mapCellStateProvider,
                                          IItemStringService itemStringService,
                                          IItemNameColorService itemNameColorService,
                                          IEIFFileProvider eifFileProvider,
                                          ICurrentMapProvider currentMapProvider,
                                          IMapInteractionController mapInteractionController,
                                          IUserInputProvider userInputProvider,
                                          IActiveDialogProvider activeDialogProvider,
                                          IContextMenuProvider contextMenuProvider,
                                          IClientWindowSizeProvider clientWindowSizeProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterProvider = characterProvider;
            _characterRendererProvider = characterRendererProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
            _mapCellStateProvider = mapCellStateProvider;
            _itemStringService = itemStringService;
            _itemNameColorService = itemNameColorService;
            _eifFileProvider = eifFileProvider;
            _currentMapProvider = currentMapProvider;
            _mapInteractionController = mapInteractionController;
            _userInputProvider = userInputProvider;
            _activeDialogProvider = activeDialogProvider;
            _contextMenuProvider = contextMenuProvider;
            _clientWindowSizeProvider = clientWindowSizeProvider;
        }

        public IMouseCursorRenderer Create()
        {
            return new MouseCursorRenderer(_nativeGraphicsManager,
                                           _characterProvider,
                                           _characterRendererProvider,
                                           _renderOffsetCalculator,
                                           _mapCellStateProvider,
                                           _itemStringService,
                                           _itemNameColorService,
                                           _eifFileProvider,
                                           _currentMapProvider,
                                           _mapInteractionController,
                                           _userInputProvider,
                                           _activeDialogProvider,
                                           _contextMenuProvider,
                                           _clientWindowSizeProvider);
        }
    }

    public interface IMouseCursorRendererFactory
    {
        IMouseCursorRenderer Create();
    }
}
