// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.HUD
{
    public class HudButtonController : IHudButtonController
    {
        private readonly IHudStateActions _hudStateActions;

        public HudButtonController(IHudStateActions hudStateActions)
        {
            _hudStateActions = hudStateActions;
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
            _hudStateActions.SwitchToState(InGameStates.Help);
        }
    }
}