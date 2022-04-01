using System;
using System.Collections.Generic;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Panels.Old;
using EndlessClient.Input;
using EndlessClient.Old;
using EndlessClient.UIControls;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using EOLib.IO.Pub;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using XNAControls.Old;

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
        private readonly OldEOPartyPanel m_party;
        private OldActiveSpells activeSpells;

        private ChatTextBox chatTextBox;

        private readonly XNAButton m_expInfo, m_questInfo;

        public DateTime SessionStartTime { get; private set; }

        public HUD(Game g, PacketAPI api) : base(g)
        {
            if(!api.Initialized)
                throw new ArgumentException("Need to initialize connection before the in-game stuff will work");
            m_packetAPI = api;

            DrawOrder = 100;

            chatRenderer = new OldChatRenderer();

            CreateChatTextbox();

            //m_party = new OldEOPartyPanel(pnlParty);

            m_expInfo = new XNAButton(((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 58),
                new Vector2(55, 0),
                new Rectangle(331, 30, 22, 14),
                new Rectangle(331, 30, 22, 14)) {DrawOrder = HUD_CONTROL_DRAW_ORDER};
            m_expInfo.OnClick += (o, e) => Dialogs.Old.SessionExpDialog.Show();
            m_questInfo = new XNAButton(((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 58),
                new Vector2(77, 0),
                new Rectangle(353, 30, 22, 14),
                new Rectangle(353, 30, 22, 14)) {DrawOrder = HUD_CONTROL_DRAW_ORDER};
            m_questInfo.OnClick += (o, e) => Dialogs.Old.QuestProgressDialog.Show(m_packetAPI);

            //no need to make this a member variable
            //it does not have any resources to dispose and it is automatically disposed by the framework
            // ReSharper disable once UnusedVariable
            //OldEOSettingsPanel settings = new OldEOSettingsPanel(pnlSettings);
        }

        #region Constructor Helpers

        private void CreateChatTextbox()
        {
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

        #endregion

        public override void Initialize()
        {
            OldWorld.Instance.ActiveMapRenderer.Visible = true;
            if (!Game.Components.Contains(OldWorld.Instance.ActiveMapRenderer))
                Game.Components.Add(OldWorld.Instance.ActiveMapRenderer);
            OldWorld.Instance.ActiveCharacterRenderer.Visible = true;

            //the draw orders are adjusted for child items in the constructor.
            //calling SetParent will break this.

            //activeSpells = new OldActiveSpells(pnlActiveSpells, m_packetAPI);
            activeSpells.Initialize();
            
            SessionStartTime = DateTime.Now;

            base.Initialize();
        }

        #region Helper Methods

        #endregion

        #region Public Interface for classes outside HUD

        public void AddChat(ChatTab whichTab, string who, string message, ChatIcon chatIcon = ChatIcon.None, ChatColor chatColor = ChatColor.Default)
        {
            chatRenderer.AddTextToTab(whichTab, who, message, chatIcon, chatColor);
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

        public void RefreshStats()
        {
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

                chatRenderer.Dispose();

                m_expInfo.Close();
                m_questInfo.Close();
            }

            base.Dispose(disposing);
        }
    }
}
