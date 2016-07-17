// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.HUD.Panels
{
    public interface IHudPanelFactory
    {
        NewsPanel CreateNewsPanel();

        InventoryPanel CreateInventoryPanel();

        ActiveSpellsPanel CreateActiveSpellsPanel();

        PassiveSpellsPanel CreatePassiveSpellsPanel();

        ChatPanel CreateChatPanel();

        StatsPanel CreateStatsPanel();

        OnlineListPanel CreateOnlineListPanel();

        PartyPanel CreatePartyPanel();

        SettingsPanel CreateSettingsPanel();

        HelpPanel CreateHelpPanel();
    }
}
