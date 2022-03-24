using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Actions;
using EndlessClient.HUD;
using EndlessClient.Rendering.Character;
using EndlessClient.Services;
using EOLib.Graphics;

namespace EndlessClient.Rendering.Factories
{
    [AutoMappedType]
    public class ContextMenuRendererFactory : IContextMenuRendererFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IFriendIgnoreListService _friendIgnoreListService;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IContextMenuRepository _contextMenuRepository;

        public ContextMenuRendererFactory(INativeGraphicsManager nativeGraphicsManager,
            IInGameDialogActions inGameDialogActions,
            IStatusLabelSetter statusLabelSetter,
            IFriendIgnoreListService friendIgnoreListService,
            IHudControlProvider hudControlProvider,
            IContextMenuRepository contextMenuRepository)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _inGameDialogActions = inGameDialogActions;
            _statusLabelSetter = statusLabelSetter;
            _friendIgnoreListService = friendIgnoreListService;
            _hudControlProvider = hudControlProvider;
            _contextMenuRepository = contextMenuRepository;
        }

        public IContextMenuRenderer CreateContextMenuRenderer(ICharacterRenderer characterRenderer)
        {
            return new ContextMenuRenderer(_nativeGraphicsManager,
                _inGameDialogActions,
                _statusLabelSetter,
                _friendIgnoreListService,
                _hudControlProvider,
                _contextMenuRepository,
                characterRenderer);
        }
    }

    public interface IContextMenuRendererFactory
    {
        IContextMenuRenderer CreateContextMenuRenderer(ICharacterRenderer characterRenderer);
    }
}
