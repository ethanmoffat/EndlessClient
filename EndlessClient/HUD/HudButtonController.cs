using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Actions;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EOLib.Domain.Interact.Quest;
using EOLib.Domain.Online;
using EOLib.Localization;
using System.Linq;
using System.Threading.Tasks;

namespace EndlessClient.HUD
{
    [AutoMappedType]
    public class HudButtonController : IHudButtonController
    {
        private readonly IHudStateActions _hudStateActions;
        private readonly IOnlinePlayerActions _onlinePlayerActions;
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IQuestActions _questActions;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IActiveDialogProvider _activeDialogProvider;

        public HudButtonController(IHudStateActions hudStateActions,
                                   IOnlinePlayerActions onlinePlayerActions,
                                   IInGameDialogActions inGameDialogActions,
                                   IQuestActions questActions,
                                   IHudControlProvider hudControlProvider,
                                   IStatusLabelSetter statusLabelSetter,
                                   ILocalizedStringFinder localizedStringFinder,
                                   IActiveDialogProvider activeDialogProvider)
        {
            _hudStateActions = hudStateActions;
            _onlinePlayerActions = onlinePlayerActions;
            _inGameDialogActions = inGameDialogActions;
            _questActions = questActions;
            _hudControlProvider = hudControlProvider;
            _statusLabelSetter = statusLabelSetter;
            _localizedStringFinder = localizedStringFinder;
            _activeDialogProvider = activeDialogProvider;
        }

        public void ClickInventory()
        {
            _hudStateActions.SwitchToState(InGameStates.Inventory);
        }

        public void ClickViewMapToggle()
        {
            _hudStateActions.ToggleMapView();
        }

        public void ClickActiveSpells()
        {
            _hudStateActions.SwitchToState(InGameStates.ActiveSpells);
        }

        public void ClickPassiveSpells()
        {
            _hudStateActions.SwitchToState(InGameStates.PassiveSpells);
        }

        public void ClickChat()
        {
            _hudStateActions.SwitchToState(InGameStates.Chat);
        }

        public void ClickStats()
        {
            _hudStateActions.SwitchToState(InGameStates.Stats);
        }

        public async Task ClickOnlineList()
        {
            var onlinePlayers = await _onlinePlayerActions.GetOnlinePlayersAsync(fullList: true);
            _hudStateActions.SwitchToState(InGameStates.OnlineList);

            _hudControlProvider.GetComponent<OnlineListPanel>(HudControlIdentifier.OnlineListPanel)
                .UpdateOnlinePlayers(onlinePlayers);
        }

        public void ClickParty()
        {
            _hudStateActions.SwitchToState(InGameStates.Party);
        }

        public void ClickSettings()
        {
            _hudStateActions.SwitchToState(InGameStates.Settings);
        }

        public void ClickHelp()
        {
            _hudStateActions.SwitchToState(InGameStates.Help);
        }

        public async Task ClickFriendList()
        {
            _inGameDialogActions.ShowFriendListDialog();

            var onlinePlayers = await _onlinePlayerActions.GetOnlinePlayersAsync(fullList: false);
            _activeDialogProvider.FriendIgnoreDialog.MatchSome(dlg => dlg.HighlightTextByLabel(onlinePlayers.Select(x => x.Name).ToList()));

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                EOResourceID.STATUS_LABEL_FRIEND_LIST,
                _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_USE_RIGHT_MOUSE_CLICK_DELETE));
        }

        public async Task ClickIgnoreList()
        {
            _inGameDialogActions.ShowIgnoreListDialog();

            var onlinePlayers = await _onlinePlayerActions.GetOnlinePlayersAsync(fullList: false);
            _activeDialogProvider.FriendIgnoreDialog.MatchSome(dlg => dlg.HighlightTextByLabel(onlinePlayers.Select(x => x.Name).ToList()));

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                EOResourceID.STATUS_LABEL_IGNORE_LIST,
                _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_USE_RIGHT_MOUSE_CLICK_DELETE));
        }

        public void ClickSessionExp()
        {
            _inGameDialogActions.ShowSessionExpDialog();
        }

        public void ClickQuestStatus()
        {
            _questActions.RequestQuestHistory(QuestPage.Progress);
            _questActions.RequestQuestHistory(QuestPage.History);
            _inGameDialogActions.ShowQuestStatusDialog();
        }
    }
}