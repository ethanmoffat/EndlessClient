using EndlessClient.HUD.Panels.Old;
using EndlessClient.Old;
using EndlessClient.UIControls;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace EndlessClient.HUD.Controls
{
    /// <summary>
    /// Note that this is NOT an XNAControl - it is just a DrawableGameComponent
    /// </summary>
    public class HUD : DrawableGameComponent
    {
        private readonly PacketAPI m_packetAPI;

        private const int HUD_CONTROL_DRAW_ORDER = 101;

        private readonly OldEOPartyPanel m_party;

        private ChatTextBox chatTextBox;

        public DateTime SessionStartTime { get; private set; }

        public HUD(Game g, PacketAPI api) : base(g)
        {
            if(!api.Initialized)
                throw new ArgumentException("Need to initialize connection before the in-game stuff will work");
            m_packetAPI = api;

            DrawOrder = 100;

            CreateChatTextbox();

            //m_party = new OldEOPartyPanel(pnlParty);

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

            SessionStartTime = DateTime.Now;

            base.Initialize();
        }

        #region Helper Methods

        #endregion

        #region Public Interface for classes outside HUD

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

        public void SetPartyData(List<PartyMember> party) { m_party.SetData(party); }
        public void AddPartyMember(PartyMember member) { m_party.AddMember(member); }
        public void RemovePartyMember(short memberID) { m_party.RemoveMember(memberID); }
        public void CloseParty() { m_party.CloseParty(); }
        public bool MainPlayerIsInParty() { return m_party.PlayerIsMember((short)OldWorld.Instance.MainPlayer.ActiveCharacter.ID); }
        public bool PlayerIsPartyMember(short playerID) { return m_party.PlayerIsMember(playerID); }

        #endregion
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_packetAPI.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
