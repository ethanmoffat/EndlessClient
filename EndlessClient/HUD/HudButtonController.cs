using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EOLib.Domain.Interact.Quest;
using EOLib.Domain.Online;
using EOLib.Localization;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EndlessClient.HUD
{
    [AutoMappedType]
    public class HudButtonController : IHudButtonController
    {
        private readonly IHudStateActions _hudStateActions;
        private readonly IOnlinePlayerActions _onlinePlayerActions;
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IQuestActions _questActions;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public HudButtonController(IHudStateActions hudStateActions,
                                   IOnlinePlayerActions onlinePlayerActions,
                                   IInGameDialogActions inGameDialogActions,
                                   IQuestActions questActions,
                                   IStatusLabelSetter statusLabelSetter,
                                   ILocalizedStringFinder localizedStringFinder)
        {
            _hudStateActions = hudStateActions;
            _onlinePlayerActions = onlinePlayerActions;
            _inGameDialogActions = inGameDialogActions;
            _questActions = questActions;
            _statusLabelSetter = statusLabelSetter;
            _localizedStringFinder = localizedStringFinder;
        }

        public void ShowNews()
        {
            _hudStateActions.SwitchToState(InGameStates.News);
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

        public void ClickOnlineList()
        {
            _onlinePlayerActions.RequestOnlinePlayers(fullList: true);
            _hudStateActions.SwitchToState(InGameStates.OnlineList);
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
            var panel = _hudStateActions.SwitchToState(InGameStates.Help);
            if (panel.Visible)
            {
                _inGameDialogActions.ShowHelpDialog();
            }
        }

        public void ClickFriendList()
        {
            _inGameDialogActions.ShowFriendListDialog();
            _onlinePlayerActions.RequestOnlinePlayers(fullList: false);

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                EOResourceID.STATUS_LABEL_FRIEND_LIST,
                _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_USE_RIGHT_MOUSE_CLICK_DELETE));
        }

        public void ClickIgnoreList()
        {
            _inGameDialogActions.ShowIgnoreListDialog();
            _onlinePlayerActions.RequestOnlinePlayers(fullList: false);

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
