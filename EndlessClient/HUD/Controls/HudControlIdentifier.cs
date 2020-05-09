using System;

namespace EndlessClient.HUD.Controls
{
    public enum HudControlIdentifier
    {
        CurrentKeyStateTracker = int.MinValue, //this should always be first!

        MapRenderer = 0,

        HudBackground,

        //buttons and panels
        InventoryButton,
        InventoryPanel,

        ViewMapButton,

        ActiveSpellsButton,
        ActiveSpellsPanel,

        PassiveSpellsButton,
        PassiveSpellsPanel,

        ChatButton,
        ChatPanel,

        StatsButton,
        StatsPanel,

        OnlineListButton,
        OnlineListPanel,

        PartyButton,
        PartyPanel,

        MacroButton,
        
        SettingsButton,
        SettingsPanel,

        HelpButton,
        HelpPanel,

        NewsPanel,

        //top bar
        UsageAndStatsButton,
        QuestsButton,

        HPStatusBar,
        TPStatusBar,
        SPStatusBar,
        TNLStatusBar,

        //mid stuff
        ChatModePictureBox,
        ChatTextBox,

        FriendList,
        IgnoreList,

        //lower stuff
        StatusLabel,
        ClockLabel,

        //not displayed
        UsageTracker,
        UserInputHandler,
        CharacterAnimator,
        NPCAnimator,

        PreviousKeyStateTracker = Int32.MaxValue //this should always be last!
    }
}
