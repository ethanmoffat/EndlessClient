// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.HUD
{
	public interface IHudButtonController
	{
		void SetInitialHudState();

		void ClickInventory();

		void ClickShowMap();

		void ClickActiveSpells();

		void ClickPassiveSpells();

		void ClickChat();

		void ClickStats();

		void ClickWhoIsOnline();

		void ClickParty();

		//void ClickMacro();

		void ClickSettings();

		void ClickHelp();

		//friend/ignore

		//E/Q
	}
}
