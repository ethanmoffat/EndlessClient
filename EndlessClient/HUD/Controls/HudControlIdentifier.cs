using System;

namespace EndlessClient.HUD.Controls;

public enum HudControlIdentifier
{
    CurrentUserInputTracker = int.MinValue, //this should always be first!

    MapRenderer = 0,
    StatusIcons,
    MiniMapRenderer,

    ClickDispatcher,

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
    SessionExpButton,
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
    PeriodicStatUpdater,
    UserInputHandler,
    CharacterAnimator,
    NPCAnimator,
    UnknownEntitiesRequester,
    PeriodicEmoteHandler,

    PreviousUserInputTracker = Int32.MaxValue, //this should always be last!
}