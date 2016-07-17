// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.HUD
{
    public interface IHudButtonController
    {
        void ClickInventory();

        void ClickViewMapToggle();

        void ClickActiveSpells();

        void ClickPassiveSpells();

        void ClickChat();

        void ClickStats();

        void ClickOnlineList();

        void ClickParty();

        //void ClickMacro();

        void ClickSettings();

        void ClickHelp();

        //friend/ignore

        //E/Q
    }
}
