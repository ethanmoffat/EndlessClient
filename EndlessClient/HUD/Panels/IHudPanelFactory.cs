namespace EndlessClient.HUD.Panels;

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