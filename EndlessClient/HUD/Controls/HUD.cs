﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Threading;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Panels.Old;
using EndlessClient.HUD.StatusBars;
using EndlessClient.Input;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using EOLib.IO.Pub;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using XNAControls;
using ChatType = EOLib.Domain.Chat.ChatType;

namespace EndlessClient.HUD.Controls
{
    /// <summary>
    /// Note that this is NOT an XNAControl - it is just a DrawableGameComponent
    /// </summary>
    public class HUD : DrawableGameComponent
    {
        private readonly PacketAPI m_packetAPI;

        private const int HUD_CONTROL_DRAW_ORDER = 101;

        private readonly OldChatRenderer chatRenderer;
        private OldEOInventory inventory;
        private OldEOCharacterStats stats;
        private readonly OldEOOnlineList m_whoIsOnline;
        private readonly OldEOPartyPanel m_party;
        private OldActiveSpells activeSpells; 

        private Timer m_muteTimer;

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

            chatRenderer = new OldChatRenderer();
            //chatRenderer.SetParent(pnlChat);
            chatRenderer.AddTextToTab(ChatTab.Global, OldWorld.GetString(EOResourceID.STRING_SERVER),
                OldWorld.GetString(EOResourceID.GLOBAL_CHAT_SERVER_MESSAGE_1),
                ChatIcon.Note, ChatColor.Server);
            chatRenderer.AddTextToTab(ChatTab.Global, OldWorld.GetString(EOResourceID.STRING_SERVER),
                OldWorld.GetString(EOResourceID.GLOBAL_CHAT_SERVER_MESSAGE_2),
                ChatIcon.Note, ChatColor.Server);

            CreateChatTextbox();

            m_muteTimer = new Timer(s =>
            {
                chatTextBox.ToggleTextInputIgnore();
                //currentChatMode = ChatMode.NoText;
                m_muteTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }, null, Timeout.Infinite, Timeout.Infinite);

            //m_whoIsOnline = new OldEOOnlineList(pnlOnline);
            //m_party = new OldEOPartyPanel(pnlParty);

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
            m_friendList.OnMouseOver += (o, e) => SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_BUTTON, EOResourceID.STATUS_LABEL_FRIEND_LIST);

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
            m_ignoreList.OnMouseOver += (o, e) => SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_BUTTON, EOResourceID.STATUS_LABEL_IGNORE_LIST);

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
            //OldEOSettingsPanel settings = new OldEOSettingsPanel(pnlSettings);
        }

        #region Constructor Helpers

        private void CreateChatTextbox()
        {
            chatTextBox.OnEnterPressed += _doTalk;

            chatTextBox.OnTextChanged += (s, e) =>
            {
                if (chatTextBox.Text.Length == 1 && chatTextBox.Text[0] == '~' &&
                    OldWorld.Instance.MainPlayer.ActiveCharacter.CurrentMap == OldWorld.Instance.JailMap)
                {
                    chatTextBox.Text = "";
                    SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.JAIL_WARNING_CANNOT_USE_GLOBAL);
                }
            };
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

            //the draw orders are adjusted for child items in the constructor.
            //calling SetParent will break this.
            //inventory = new OldEOInventory(pnlInventory, m_packetAPI);

            //stats = new OldEOCharacterStats(pnlStats);
            stats.Initialize();

            //activeSpells = new OldActiveSpells(pnlActiveSpells, m_packetAPI);
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

        #region Helper Methods

        private void _doTalk(object sender, EventArgs e)
        {
            if (chatTextBox.Text.Length <= 0)
                return;

            string chatText = chatTextBox.Text;
            chatTextBox.Text = "";
            switch (chatText[0])
            {
                case '!':  //private talk
                {
                    string character, message;
                    if (chatRenderer.SelectedTab.WhichTab == ChatTab.Private1 || chatRenderer.SelectedTab.WhichTab == ChatTab.Private2)
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

                    var filtered = OldChatRenderer.Filter(message, true);
                    if (filtered != null)
                    {
                        //if (!m_packetAPI.Speak(ChatType.PM, message, character))
                        //{
                        //    EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
                        //    break;
                        //}

                        var whichPrivateChat = chatRenderer.StartConversation(character);
                        //the other player will have their messages rendered in Color.PM on scr
                        //this player will have their messages rendered in Color.PM on the PM tab
                        AddChat(whichPrivateChat, OldWorld.Instance.MainPlayer.ActiveCharacter.Name, filtered, ChatIcon.Note, ChatColor.PM);
                    }
                }
                    break;
            }
        }

        #endregion

        #region Public Interface for classes outside HUD

        public void AddChat(ChatTab whichTab, string who, string message, ChatIcon chatIcon = ChatIcon.None, ChatColor chatColor = ChatColor.Default)
        {
            chatRenderer.AddTextToTab(whichTab, who, message, chatIcon, chatColor);
        }

        public void PrivatePlayerNotFound(string character)
        {
            string endPart = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized2].Data[(int) EOResourceID.SYS_CHAT_PM_PLAYER_COULD_NOT_BE_FOUND];
            //add message to Sys and close the chat that was opened for 'character'
            //this is how original client does it - you can see the PM tab open/close really quickly
            chatRenderer.ClosePrivateChat(character);
            AddChat(ChatTab.System, "", string.Format("{0} " + endPart, character), ChatIcon.Error, ChatColor.Error);
        }

        public void SetMuted()
        {
            //currentChatMode = ChatMode.Muted;
            chatTextBox.ToggleTextInputIgnore();
            m_muteTimer.Change(Constants.MuteDefaultTimeMinutes*60000, 0);
        }

        public ChatTab GetPrivateChatTab(string character)
        {
            return chatRenderer.StartConversation(character);
        }

        public void SetChatText(string text)
        {
            chatTextBox.Text = text;
        }

        public void SetStatusLabel(EOResourceID type, EOResourceID message, string extra = "")
        {
        }

        public void SetStatusLabel(EOResourceID type, string extra, EOResourceID message)
        {
            //SetStatusLabelText(string.Format("[ {0} ] {1} {2}", typeText, extra, messageText));
        }

        public void SetStatusLabel(EOResourceID type, string detail)
        {
            //SetStatusLabelText(string.Format("[ {0} ] {1}", typeText, detail));
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
        public ESFRecord GetSpellFromIndex(int index) { return activeSpells.GetSpellRecordBySlot(index); }
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

                chatTextBox.Close();

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
