using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EOLib.Domain.Online;

namespace EndlessClient.HUD
{
    [MappedType(BaseType = typeof(IHudButtonController))]
    public class HudButtonController : IHudButtonController
    {
        private readonly IHudStateActions _hudStateActions;
        private readonly IOnlinePlayerActions _onlinePlayerActions;
        private readonly IHudControlProvider _hudControlProvider;

        public HudButtonController(IHudStateActions hudStateActions,
                                   IOnlinePlayerActions onlinePlayerActions,
                                   IHudControlProvider hudControlProvider)
        {
            _hudStateActions = hudStateActions;
            _onlinePlayerActions = onlinePlayerActions;
            _hudControlProvider = hudControlProvider;
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

        public async void ClickOnlineList()
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
    }
}