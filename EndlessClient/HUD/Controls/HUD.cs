// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Panels;
using EndlessClient.HUD.StatusBars;
using EndlessClient.Input;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.HUD.Controls
{
    /// <summary>
    /// Note that this is NOT an XNAControl - it is just a DrawableGameComponent
    /// </summary>
    public class HUD : DrawableGameComponent
    {
        private readonly PacketAPI m_packetAPI;

        private const int HUD_CONTROL_DRAW_ORDER = 101;

        //todo: remove panels when refactored
        private XNAPanel pnlInventory;
        private XNAPanel pnlActiveSpells;
        private XNAPanel pnlPassiveSpells;
        private XNAPanel pnlChat;
        private XNAPanel pnlStats;
        private XNAPanel pnlNews;
        private XNAPanel pnlOnline;
        private XNAPanel pnlParty;
        private XNAPanel pnlSettings;
        private XNAPanel pnlHelp;
        private readonly SpriteBatch SpriteBatch;
        private readonly EOChatRenderer chatRenderer;
        private EOInventory inventory;
        private EOCharacterStats stats;
        private readonly EOOnlineList m_whoIsOnline;
        private readonly EOPartyPanel m_party;
        private ActiveSpells activeSpells; 

        private readonly XNALabel statusLabel;
        private Timer m_muteTimer;

        private InGameStates state;
        private ChatMode currentChatMode;
        private Texture2D modeTexture;
        private bool modeTextureLoaded;
        private ChatTextBox chatTextBox;

        private readonly XNAButton m_friendList, m_ignoreList, m_expInfo, m_questInfo;

        public DateTime SessionStartTime { get; private set; }

        private List<InputKeyListenerBase> m_inputListeners;
        
        public HUD(Game g, PacketAPI api) : base(g)
        {
            if(!api.Initialized)
                throw new ArgumentException("Need to initialize connection before the in-game stuff will work");
            m_packetAPI = api;

            DrawOrder = 100;

            SpriteBatch = new SpriteBatch(g.GraphicsDevice);

            state = InGameStates.News;

            chatRenderer = new EOChatRenderer();
            chatRenderer.SetParent(pnlChat);
            chatRenderer.AddTextToTab(ChatTabs.Global, OldWorld.GetString(DATCONST2.STRING_SERVER),
                OldWorld.GetString(DATCONST2.GLOBAL_CHAT_SERVER_MESSAGE_1),
                ChatType.Note, ChatColor.Server);
            chatRenderer.AddTextToTab(ChatTabs.Global, OldWorld.GetString(DATCONST2.STRING_SERVER),
                OldWorld.GetString(DATCONST2.GLOBAL_CHAT_SERVER_MESSAGE_2),
                ChatType.Note, ChatColor.Server);

            CreateChatTextbox();

            m_muteTimer = new Timer(s =>
            {
                chatTextBox.ToggleTextInputIgnore();
                currentChatMode = ChatMode.NoText;
                m_muteTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }, null, Timeout.Infinite, Timeout.Infinite);

            statusLabel = new XNALabel(new Rectangle(97, 455, 1, 1), Constants.FontSize07) { DrawOrder = HUD_CONTROL_DRAW_ORDER };

            m_whoIsOnline = new EOOnlineList(pnlOnline);
            m_party = new EOPartyPanel(pnlParty);

            m_friendList = new XNAButton(((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 27, false, true),
                new Vector2(592, 312),
                new Rectangle(0, 260, 17, 15),
                new Rectangle(0, 276, 17, 15))
            {
                Visible = true,
                Enabled = true,
                DrawOrder = HUD_CONTROL_DRAW_ORDER
            };
            m_friendList.OnClick += (o, e) => FriendIgnoreListDialog.Show(isIgnoreList: false, apiHandle: m_packetAPI);
            m_friendList.OnMouseOver += (o, e) => SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_BUTTON, DATCONST2.STATUS_LABEL_FRIEND_LIST);

            m_ignoreList = new XNAButton(((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 27, false, true),
                new Vector2(609, 312),
                new Rectangle(17, 260, 17, 15),
                new Rectangle(17, 276, 17, 15))
            {
                Visible = true,
                Enabled = true,
                DrawOrder = HUD_CONTROL_DRAW_ORDER
            };
            m_ignoreList.OnClick += (o, e) => FriendIgnoreListDialog.Show(isIgnoreList: true, apiHandle: m_packetAPI);
            m_ignoreList.OnMouseOver += (o, e) => SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_BUTTON, DATCONST2.STATUS_LABEL_IGNORE_LIST);

            m_expInfo = new XNAButton(((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 58),
                new Vector2(55, 0),
                new Rectangle(331, 30, 22, 14),
                new Rectangle(331, 30, 22, 14)) {DrawOrder = HUD_CONTROL_DRAW_ORDER};
            m_expInfo.OnClick += (o, e) => SessionExpDialog.Show();
            m_questInfo = new XNAButton(((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 58),
                new Vector2(77, 0),
                new Rectangle(353, 30, 22, 14),
                new Rectangle(353, 30, 22, 14)) {DrawOrder = HUD_CONTROL_DRAW_ORDER};
            m_questInfo.OnClick += (o, e) => QuestProgressDialog.Show(m_packetAPI);

            //no need to make this a member variable
            //it does not have any resources to dispose and it is automatically disposed by the framework
            // ReSharper disable once UnusedVariable
            EOSettingsPanel settings = new EOSettingsPanel(pnlSettings);
        }

        #region Constructor Helpers

        private void CreateChatTextbox()
        {
            chatTextBox = new ChatTextBox(new Rectangle(124, 308, 440, 19), Game.Content.Load<Texture2D>("cursor"),
                Constants.FontSize08)
            {
                Selected = true,
                Visible = true,
                MaxChars = 140,
                DrawOrder = HUD_CONTROL_DRAW_ORDER
            };
            OldWorld.IgnoreDialogs(chatTextBox);
            chatTextBox.OnEnterPressed += _doTalk;
            chatTextBox.OnClicked += (s, e) =>
            {
                //make sure clicking on the textarea selects it (this is an annoying problem in the original client)
                if (((EOGame) Game).Dispatcher.Subscriber != null)
                    ((XNATextBox) ((EOGame) Game).Dispatcher.Subscriber).Selected = false;

                ((EOGame) Game).Dispatcher.Subscriber = chatTextBox;
                chatTextBox.Selected = true;
            };
            chatTextBox.OnTextChanged += (s, e) =>
            {
                if (chatTextBox.Text.Length <= 0)
                {
                    if (modeTextureLoaded && modeTexture != null)
                    {
                        modeTextureLoaded = false;
                        modeTexture.Dispose();
                        modeTexture = null;

                        currentChatMode = ChatMode.NoText;
                    }
                    return;
                }

                if (chatTextBox.Text.Length == 1 && chatTextBox.Text[0] == '~' &&
                    OldWorld.Instance.MainPlayer.ActiveCharacter.CurrentMap == OldWorld.Instance.JailMap)
                {
                    chatTextBox.Text = "";
                    SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.JAIL_WARNING_CANNOT_USE_GLOBAL);
                    return;
                }

                switch (chatTextBox.Text[0])
                {
                    case '!':
                        currentChatMode = ChatMode.Private;
                        break;
                    case '@': //should show global if admin, otherwise, public/normal chat
                        if (OldWorld.Instance.MainPlayer.ActiveCharacter.AdminLevel == AdminLevel.Player)
                            goto default;
                        currentChatMode = ChatMode.Global;
                        break;
                    case '~':
                        currentChatMode = ChatMode.Global;
                        break;
                    case '+':
                    {
                        if (OldWorld.Instance.MainPlayer.ActiveCharacter.AdminLevel == AdminLevel.Player)
                            goto default;
                        currentChatMode = ChatMode.Admin;
                    }
                        break;
                    case '\'':
                        currentChatMode = ChatMode.Group;
                        break;
                    case '&':
                    {
                        if (OldWorld.Instance.MainPlayer.ActiveCharacter.GuildName == "")
                            goto default;
                        currentChatMode = ChatMode.Guild;
                    }
                        break;
                    default:
                        currentChatMode = ChatMode.Public;
                        break;
                }
            };

            ((EOGame) Game).Dispatcher.Subscriber = chatTextBox;
        }

        private void CreateStatusBars()
        {
            var hp = new HPStatusBar { DrawOrder = HUD_CONTROL_DRAW_ORDER };
            var tp = new TPStatusBar { DrawOrder = HUD_CONTROL_DRAW_ORDER };
            var sp = new SPStatusBar { DrawOrder = HUD_CONTROL_DRAW_ORDER };
            var tnl = new TNLStatusBar { DrawOrder = HUD_CONTROL_DRAW_ORDER };

            if (!Game.Components.Contains(hp) || !Game.Components.Contains(tp) ||
                !Game.Components.Contains(sp) || !Game.Components.Contains(tnl))
                throw new InvalidOperationException("One of the status bars (HP, SP, TP, or TNL) is not in the game components list.");
        }

        #endregion

        public override void Initialize()
        {
            OldWorld.Instance.ActiveMapRenderer.Visible = true;
            if (!Game.Components.Contains(OldWorld.Instance.ActiveMapRenderer))
                Game.Components.Add(OldWorld.Instance.ActiveMapRenderer);
            OldWorld.Instance.ActiveCharacterRenderer.Visible = true;
            
            //todo: figure out a good way to do status label setting
                    //if (statusStartTime.HasValue && (DateTime.Now - statusStartTime.Value).TotalMilliseconds > 3000)
                    //{
                    //    SetStatusLabelText("");
                    //    m_statusRecentlySet = false;
                    //    statusStartTime = null;
                    //}

            //the draw orders are adjusted for child items in the constructor.
            //calling SetParent will break this.
            inventory = new EOInventory(pnlInventory, m_packetAPI);

            stats = new EOCharacterStats(pnlStats);
            stats.Initialize();

            activeSpells = new ActiveSpells(pnlActiveSpells, m_packetAPI);
            activeSpells.Initialize();
            
            SessionStartTime = DateTime.Now;

            m_inputListeners = new List<InputKeyListenerBase>(4)
            {
                new FunctionKeyListener(),
                new ArrowKeyListener(),
                new ControlKeyListener(),
                new NumPadListener()
            };
            m_inputListeners.ForEach(x => x.InputTimeUpdated += OldWorld.Instance.ActiveCharacterRenderer.UpdateInputTime);

            CreateStatusBars();

            base.Initialize();
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.Begin();

            //show the little graphic next
            if (currentChatMode != ChatMode.NoText && !modeTextureLoaded)
            {
                Texture2D chatModeTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 31);
                int oneMode = chatModeTexture.Height/8;
                Color[] data = new Color[chatModeTexture.Width*oneMode]; //there are 8 chat mode graphics in the texture
                chatModeTexture.GetData(0, new Rectangle(0, (int) currentChatMode*oneMode, chatModeTexture.Width, oneMode), data, 0,
                    data.Length);
                modeTexture = new Texture2D(Game.GraphicsDevice, chatModeTexture.Width, oneMode);
                modeTexture.SetData(data);
                modeTextureLoaded = true;
            }

            if(modeTextureLoaded && modeTexture != null)
                SpriteBatch.Draw(modeTexture, new Vector2(16, 309), Color.White);

            try
            {
                SpriteBatch.End();
            }
            catch (ObjectDisposedException) { return; }

            base.Draw(gameTime);
        }

        #region Helper Methods

        private void _doStateChange(InGameStates newGameState)
        {
            state = newGameState;

            pnlNews.Visible = false;

            pnlInventory.Visible = false;
            pnlActiveSpells.Visible = false;
            pnlPassiveSpells.Visible = false;
            pnlChat.Visible = false;
            pnlStats.Visible = false;

            pnlOnline.Visible = false;
            pnlParty.Visible = false;
            pnlSettings.Visible = false;
            pnlHelp.Visible = false;

            switch(state)
            {
                case InGameStates.Inventory:
                    pnlInventory.Visible = true;
                    break;
                case InGameStates.ActiveSpells:
                    pnlActiveSpells.Visible = true;
                    break;
                case InGameStates.PassiveSpells:
                    pnlPassiveSpells.Visible = true;
                    break;
                case InGameStates.Chat:
                    pnlChat.Visible = true;
                    SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_CHAT_PANEL_NOW_VIEWED);
                    break;
                case InGameStates.Stats:
                    stats.Refresh();
                    pnlStats.Visible = true;
                    SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_STATS_PANEL_NOW_VIEWED);
                    break;
                case InGameStates.OnlineList:
                    List<OnlineEntry> onlineList;
                    if (!m_packetAPI.RequestOnlinePlayers(true, out onlineList))
                        EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
                    m_whoIsOnline.SetOnlinePlayerList(onlineList);
                    pnlOnline.Visible = true;
                    SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_ONLINE_PLAYERS_NOW_VIEWED);
                    break;
                case InGameStates.Party:
                    pnlParty.Visible = true;
                    break;
                case InGameStates.Settings:
                    pnlSettings.Visible = true;
                    break;
                case InGameStates.Help:
                    pnlHelp.Visible = true;
                    SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_HUD_BUTTON_HOVER_LAST);
                    break;
            }
        }

        /// <summary>
        /// Event for enter keypress of primary textbox. Does the chat
        /// </summary>
        private void _doTalk(object sender, EventArgs e)
        {
            if (chatTextBox.Text.Length <= 0)
                return;

            string chatText = chatTextBox.Text, filtered;
            chatTextBox.Text = "";
            switch (chatText[0])
            {
                case '+':  //admin talk
                    if (OldWorld.Instance.MainPlayer.ActiveCharacter.AdminLevel == AdminLevel.Player)
                        goto default;
                    filtered = EOChatRenderer.Filter(chatText.Substring(1), true);
                    if (filtered != null)
                    {
                        if (!m_packetAPI.Speak(TalkType.Admin, chatText.Substring(1)))
                        {
                            _returnToLogin();
                            break;
                        }
                        AddChat(ChatTabs.Group, OldWorld.Instance.MainPlayer.ActiveCharacter.Name, filtered, ChatType.HGM, ChatColor.Admin);
                    }
                    break;
                case '@': //system talk (admin)
                    if (OldWorld.Instance.MainPlayer.ActiveCharacter.AdminLevel == AdminLevel.Player)
                        goto default;
                    filtered = EOChatRenderer.Filter(chatText.Substring(1), true);
                    if (filtered != null)
                    {
                        if (!m_packetAPI.Speak(TalkType.Announce, chatText.Substring(1)))
                        {
                            _returnToLogin();
                            break;
                        }
                        OldWorld.Instance.ActiveMapRenderer.MakeSpeechBubble(null, filtered, false);
                        string name = OldWorld.Instance.MainPlayer.ActiveCharacter.Name;
                        AddChat(ChatTabs.Local, name, filtered, ChatType.GlobalAnnounce, ChatColor.ServerGlobal);
                        AddChat(ChatTabs.Global, name, filtered, ChatType.GlobalAnnounce, ChatColor.ServerGlobal);
                        AddChat(ChatTabs.Group, name, filtered, ChatType.GlobalAnnounce, ChatColor.ServerGlobal);
                    }
                    break;
                case '\'': //group talk
                    if (!m_party.PlayerIsMember((short) OldWorld.Instance.MainPlayer.ActiveCharacter.ID))
                        break; //not in a party, cancel the talk
                    filtered = EOChatRenderer.Filter(chatText.Substring(1), true);
                    if (filtered != null)
                    {
                        if (!m_packetAPI.Speak(TalkType.Party, chatText.Substring(1)))
                        {
                            _returnToLogin();
                            break;
                        }
                        OldWorld.Instance.ActiveMapRenderer.MakeSpeechBubble(null, filtered, true);
                        AddChat(ChatTabs.Local, OldWorld.Instance.MainPlayer.ActiveCharacter.Name, filtered, ChatType.PlayerPartyDark, ChatColor.PM);
                        AddChat(ChatTabs.Group, OldWorld.Instance.MainPlayer.ActiveCharacter.Name, filtered, ChatType.PlayerPartyDark);
                    }
                    break;
                case '&':  //guild talk
                    if (OldWorld.Instance.MainPlayer.ActiveCharacter.GuildName == "")
                        goto default;
                    
                    filtered = EOChatRenderer.Filter(chatText.Substring(1), true);
                    if (filtered != null)
                    {
                        if (!m_packetAPI.Speak(TalkType.Guild, chatText.Substring(1)))
                        {
                            _returnToLogin();
                            break;
                        }
                        //note: more processing of colors/icons is needed here
                        AddChat(ChatTabs.Group, OldWorld.Instance.MainPlayer.ActiveCharacter.Name, filtered);
                    }
                    break;
                case '~':  //global talk
                    filtered = EOChatRenderer.Filter(chatText.Substring(1), true);
                    if (filtered != null)
                    {
                        if (!m_packetAPI.Speak(TalkType.Global, chatText.Substring(1)))
                        {
                            _returnToLogin();
                            break;
                        }
                        AddChat(ChatTabs.Global, OldWorld.Instance.MainPlayer.ActiveCharacter.Name, filtered);
                    }
                    break;
                case '!':  //private talk
                {
                    string character, message;
                    if (chatRenderer.SelectedTab.WhichTab == ChatTabs.Private1 || chatRenderer.SelectedTab.WhichTab == ChatTabs.Private2)
                    {
                        character = chatRenderer.SelectedTab.ChatCharacter;
                        message = chatText.Substring(1);
                    }
                    else
                    {
                        int firstSpace = chatText.IndexOf(' ');
                        if (firstSpace < 7) return; //character names should be 6, leading ! should be 1, 6+1=7 and THAT'S MATH
                        character = chatText.Substring(1, firstSpace - 1);
                        message = chatText.Substring(firstSpace + 1);
                    }

                    character = character.Substring(0, 1).ToUpper() + character.Substring(1).ToLower();

                    filtered = EOChatRenderer.Filter(message, true);
                    if (filtered != null)
                    {
                        if (!m_packetAPI.Speak(TalkType.PM, message, character))
                        {
                            _returnToLogin();
                            break;
                        }

                        ChatTabs whichPrivateChat = chatRenderer.StartConversation(character);
                        //the other player will have their messages rendered in Color.PM on scr
                        //this player will have their messages rendered in Color.PM on the PM tab
                        if (whichPrivateChat != ChatTabs.None)
                        {
                            AddChat(whichPrivateChat, OldWorld.Instance.MainPlayer.ActiveCharacter.Name, filtered, ChatType.Note, ChatColor.PM);
                        }
                    }
                }
                    break;
                case '#':  //local command
                {
                    string cmd = chatText.Substring(1).ToLower().Trim();
                    string[] args = cmd.Split(' ');
                    
                    if (args.Length == 1 && args[0] == "nowall")
                    {
                        OldWorld.Instance.ActiveCharacterRenderer.NoWall = !OldWorld.Instance.ActiveCharacterRenderer.NoWall;
                    }
                    else if (args.Length == 2 && args[0] == "find")
                    {
                        if(!m_packetAPI.FindPlayer(args[1]))
                            ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                    }
                    else if (args.Length == 1 && args[0] == "loc")
                    {
                        string firstPart = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized2].Data[(int) DATCONST2.STATUS_LABEL_YOUR_LOCATION_IS_AT];
                        AddChat(ChatTabs.Local, "System", string.Format(firstPart + " {0}  x:{1}  y:{2}",
                            OldWorld.Instance.ActiveMapRenderer.MapRef.Properties.MapID,
                            OldWorld.Instance.MainPlayer.ActiveCharacter.X,
                            OldWorld.Instance.MainPlayer.ActiveCharacter.Y),
                            ChatType.LookingDude);
                    }
                    else if (args.Length == 1 && cmd == "usage")
                    {
                        int usage = OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.Usage;
                        AddChat(ChatTabs.Local, "System", string.Format("[x] usage: {0}hrs. {1}min.", usage/60, usage%60));
                    }
                    else if (args.Length == 1 && cmd == "ping")
                    {
                        if (!m_packetAPI.PingServer())
                            ((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                    }
                }
                    break;
                default:
                {
                    filtered = EOChatRenderer.Filter(chatText, true);
                    if (filtered != null)
                    {
                        //send packet to the server
                        if (!m_packetAPI.Speak(TalkType.Local, chatText))
                        {
                            _returnToLogin();
                            break;
                        }

                        //do the rendering
                        OldWorld.Instance.ActiveMapRenderer.MakeSpeechBubble(null, filtered, false);
                        AddChat(ChatTabs.Local, OldWorld.Instance.MainPlayer.ActiveCharacter.Name, filtered);
                    }
                }
                    break;
            }
        }

        private void _returnToLogin()
        {
            //any other logic prior to disconnecting goes here
            EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
        }

        private void SetStatusLabelText(string text)
        {
            statusLabel.Text = text;
        }

        private void CheckStatusLabelType(DATCONST2 type)
        {
            switch (type)
            {
                case DATCONST2.STATUS_LABEL_TYPE_ACTION:
                case DATCONST2.STATUS_LABEL_TYPE_BUTTON:
                case DATCONST2.STATUS_LABEL_TYPE_INFORMATION:
                case DATCONST2.STATUS_LABEL_TYPE_WARNING:
                case DATCONST2.STATUS_LABEL_TYPE_ITEM:
                case DATCONST2.SKILLMASTER_WORD_SPELL:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", "Use either ACTION, BUTTION, INFORMATION, WARNING, or ITEM for this.");
            }
        }

        #endregion

        #region Public Interface for classes outside HUD

        public void SetNews(IList<string> lines)
        {
            if(lines.Count == 0)
                return;

            if(lines.Count == 1)
            {
                _doStateChange(InGameStates.Chat);
            }
            else
            {
                lines = lines.Where(_line => !string.IsNullOrWhiteSpace(_line)).ToList();
                //for(int i = 1; i < lines.Count; ++i)
                //{
                //    newsTab.AddText(null, lines[i], ChatType.Note);
                //    if(i != lines.Count - 1)
                //        newsTab.AddText(null, " "); //line breaks between entries
                //}
                //newsTab.SetButtonFlash();
            }
            
            chatRenderer.AddTextToTab(ChatTabs.Local, OldWorld.GetString(DATCONST2.STRING_SERVER), lines[0], ChatType.Note, ChatColor.Server);
        }

        public void AddChat(ChatTabs whichTab, string who, string message, ChatType chatType = ChatType.None, ChatColor chatColor = ChatColor.Default)
        {
            chatRenderer.AddTextToTab(whichTab, who, message, chatType, chatColor);
        }

        public void PrivatePlayerNotFound(string character)
        {
            string endPart = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized2].Data[(int) DATCONST2.SYS_CHAT_PM_PLAYER_COULD_NOT_BE_FOUND];
            //add message to Sys and close the chat that was opened for 'character'
            //this is how original client does it - you can see the PM tab open/close really quickly
            chatRenderer.ClosePrivateChat(character);
            AddChat(ChatTabs.System, "", string.Format("{0} " + endPart, character), ChatType.Error, ChatColor.Error);
        }

        public void SetMuted()
        {
            currentChatMode = ChatMode.Muted;
            chatTextBox.ToggleTextInputIgnore();
            m_muteTimer.Change(Constants.MuteDefaultTimeMinutes*60000, 0);
        }

        public ChatTabs GetPrivateChatTab(string character)
        {
            return chatRenderer.StartConversation(character);
        }

        public void SetChatText(string text)
        {
            chatTextBox.Text = text;
        }

        public void SetStatusLabel(DATCONST2 type, DATCONST2 message, string extra = "")
        {
            CheckStatusLabelType(type);

            string typeText = OldWorld.GetString(type);
            string messageText = OldWorld.GetString(message);
            SetStatusLabelText(string.Format("[ {0} ] {1} {2}", typeText, messageText, extra));
        }

        public void SetStatusLabel(DATCONST2 type, string extra, DATCONST2 message)
        {
            CheckStatusLabelType(type);

            string typeText = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized2].Data[(int)type];
            string messageText = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized2].Data[(int)message];
            SetStatusLabelText(string.Format("[ {0} ] {1} {2}", typeText, extra, messageText));
        }

        public void SetStatusLabel(DATCONST2 type, string detail)
        {
            CheckStatusLabelType(type);

            string typeText = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized2].Data[(int) type];
            SetStatusLabelText(string.Format("[ {0} ] {1}", typeText, detail));
        }

        public bool UpdateInventory(InventoryItem item)
        {
            if (item.Amount <= 0)
                inventory.RemoveItem(item.ItemID);
            else
                return inventory.UpdateItem(item);
            return true;
        }
        public bool IsInventoryDragging()
        {
            return !inventory.NoItemsDragging();
        }
        public bool InventoryFits(short id)
        {
            return inventory.ItemFits(id);
        }
        public bool ItemsFit(List<InventoryItem> newItems, List<InventoryItem> oldItems = null)
        {
            return inventory.ItemsFit(newItems, oldItems);
        }
        public void DisableEffectPotionUse() { inventory.DisableEffectPotions(); }
        public void EnableEffectPotionUse() { inventory.EnableEffectPotions(); }

        public void RefreshStats()
        {
            if(inventory != null)
                inventory.UpdateWeightLabel();
            if(stats != null)
                stats.Refresh();
            if (activeSpells != null)
                activeSpells.RefreshTotalSkillPoints();
        }

        public void SetPartyData(List<PartyMember> party) { m_party.SetData(party); }
        public void AddPartyMember(PartyMember member) { m_party.AddMember(member); }
        public void RemovePartyMember(short memberID) { m_party.RemoveMember(memberID); }
        public void CloseParty() { m_party.CloseParty(); }
        public bool MainPlayerIsInParty() { return m_party.PlayerIsMember((short)OldWorld.Instance.MainPlayer.ActiveCharacter.ID); }
        public bool PlayerIsPartyMember(short playerID) { return m_party.PlayerIsMember(playerID); }

        public void AddNewSpellToActiveSpellsByID(int spellID) { activeSpells.AddNewSpellToNextOpenSlot(spellID); }
        public SpellRecord GetSpellFromIndex(int index) { return activeSpells.GetSpellRecordBySlot(index); }
        public void SetSelectedSpell(int index) { activeSpells.SetSelectedSpellBySlot(index); }
        public void RemoveSpellFromActiveSpellsByID(int spellID) { activeSpells.RemoveSpellByID(spellID); }
        public void UpdateActiveSpellLevelByID(short spellID, short spellLevel) { activeSpells.UpdateSpellLevelByID(spellID, spellLevel); }
        public void RemoveAllSpells() { activeSpells.RemoveAllSpells(); }

        #endregion
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_packetAPI.Dispose();

                inventory.Dispose();
                chatRenderer.Dispose();
                stats.Dispose();

                if (modeTexture != null)
                    modeTexture.Dispose();
                SpriteBatch.Dispose();

                pnlInventory.Close();
                pnlActiveSpells.Close();
                pnlPassiveSpells.Close();
                pnlChat.Close();
                pnlStats.Close();
                pnlOnline.Close();
                pnlParty.Close();
                pnlSettings.Close();

                chatTextBox.Close();
                statusLabel.Close();

                m_friendList.Close();
                m_ignoreList.Close();

                m_expInfo.Close();
                m_questInfo.Close();

                if (m_muteTimer != null)
                {
                    m_muteTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_muteTimer.Dispose();
                    m_muteTimer = null;
                }

                if (m_inputListeners.Count > 0)
                {
                    m_inputListeners.ForEach(x => x.Dispose());
                    m_inputListeners.Clear();
                }
            }

            base.Dispose(disposing);
        }
    }
}
