using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Actions;
using EndlessClient.HUD;
using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EndlessClient.Services;
using EOLib.Domain.Interact;
using EOLib.Domain.Party;
using EOLib.Domain.Trade;
using EOLib.Graphics;

namespace EndlessClient.Rendering.Factories
{
    [AutoMappedType]
    public class ContextMenuRendererFactory : IContextMenuRendererFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IPaperdollActions _paperdollActions;
        private readonly IPartyActions _partyActions;
        private readonly ITradeActions _tradeActions;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IFriendIgnoreListService _friendIgnoreListService;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IContextMenuRepository _contextMenuRepository;
        private readonly IUserInputRepository _userInputRepository;
        private readonly IPartyDataProvider _partyDataProvider;

        public ContextMenuRendererFactory(INativeGraphicsManager nativeGraphicsManager,
            IInGameDialogActions inGameDialogActions,
            IPaperdollActions paperdollActions,
            IPartyActions partyActions,
            ITradeActions tradeActions,
            IStatusLabelSetter statusLabelSetter,
            IFriendIgnoreListService friendIgnoreListService,
            IHudControlProvider hudControlProvider,
            IContextMenuRepository contextMenuRepository,
            IUserInputRepository userInputRepository,
            IPartyDataProvider partyDataProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _inGameDialogActions = inGameDialogActions;
            _paperdollActions = paperdollActions;
            _partyActions = partyActions;
            _tradeActions = tradeActions;
            _statusLabelSetter = statusLabelSetter;
            _friendIgnoreListService = friendIgnoreListService;
            _hudControlProvider = hudControlProvider;
            _contextMenuRepository = contextMenuRepository;
            _userInputRepository = userInputRepository;
            _partyDataProvider = partyDataProvider;
        }

        public IContextMenuRenderer CreateContextMenuRenderer(ICharacterRenderer characterRenderer)
        {
            return new ContextMenuRenderer(_nativeGraphicsManager,
                _inGameDialogActions,
                _paperdollActions,
                _partyActions,
                _tradeActions,
                _statusLabelSetter,
                _friendIgnoreListService,
                _hudControlProvider,
                _contextMenuRepository,
                _userInputRepository,
                _partyDataProvider,
                characterRenderer);
        }
    }

    public interface IContextMenuRendererFactory
    {
        IContextMenuRenderer CreateContextMenuRenderer(ICharacterRenderer characterRenderer);
    }
}
