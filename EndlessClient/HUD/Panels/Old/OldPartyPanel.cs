using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Old;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls.Old;

namespace EndlessClient.HUD.Panels.Old
{
    public class OldEOPartyPanel : XNAControl
    {
        private readonly OldScrollBar m_scrollBar;
        private readonly XNALabel m_numMembers;
        //private List<PartyMember> m_members;
        private readonly List<XNAButton> m_buttons; 
        private bool m_mainIsLeader;
        private readonly Texture2D m_removeTexture;
        private readonly Texture2D[] m_healthBar;

        private const int DRAW_OFFSET_Y = 20,
            DRAW_ICON_X = 5,
            DRAW_NAME_X = 23,
            DRAW_LEVEL_X = 138,
            DRAW_HP_X = 205,
            DRAW_HEALTHBAR_X = 228,
            DRAW_REMOVE_X = 337;

        private const int HP_OUTLINE = 0, HP_RED = 1, HP_YELLOW = 2, HP_GREEN = 3;

        public OldEOPartyPanel(XNAPanel parent)
            : base(null, null, parent)
        {
            _setSize(parent.BackgroundImage.Width, parent.BackgroundImage.Height);

            m_removeTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 43);
            m_buttons = new List<XNAButton>();
            
            //will tint this different colors for health bar and fill a rectangle
            m_healthBar = new Texture2D[4];
            m_healthBar[HP_OUTLINE] = Game.Content.Load<Texture2D>("Party\\hp-outline");
            m_healthBar[HP_RED] = Game.Content.Load<Texture2D>("Party\\hp-red");
            m_healthBar[HP_YELLOW] = Game.Content.Load<Texture2D>("Party\\hp-yellow");
            m_healthBar[HP_GREEN] = Game.Content.Load<Texture2D>("Party\\hp-green");

            m_numMembers = new XNALabel(new Rectangle(455, 2, 27, 14), Constants.FontSize08pt5)
            {
                AutoSize = false,
                ForeColor = ColorConstants.LightGrayText,
                TextAlign = LabelAlignment.MiddleRight
            };
            m_numMembers.SetParent(this);

            m_scrollBar = new OldScrollBar(this, new Vector2(467, 20), new Vector2(16, 97), ScrollBarColors.LightOnMed)
            {
                LinesToRender = 7,
                Visible = true
            };
            m_scrollBar.SetParent(this);
            OldWorld.IgnoreDialogs(m_scrollBar);
        }

//        public void SetData(List<PartyMember> memberList)
//        {
//            if (memberList.TrueForAll(_member => _member.IsFullData))
//            {
//                if(m_members == null || m_members.Count == 0)
//                {
//                    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_PARTY_YOU_JOINED);
//                    ((EOGame)Game).Hud.AddChat(ChatTab.System, "", OldWorld.GetString(EOResourceID.STATUS_LABEL_PARTY_YOU_JOINED), ChatIcon.PlayerParty, ChatColor.PM);
//                }

//                Visible = true;
//                m_numMembers.Text = $"{memberList.Count}";
//                m_members = memberList;

//                m_mainIsLeader = m_members.FindIndex(_member => _member.IsLeader && _member.ID == OldWorld.Instance.MainPlayer.ActiveCharacter.ID) >= 0;
//                m_scrollBar.UpdateDimensions(memberList.Count);

//                m_buttons.Clear();

//                foreach (PartyMember member in m_members)
//                {
//                    _addRemoveButtonForMember(member);
//                }
//            }
//            else
//            {
//                //update HP only
//// ReSharper disable once ForCanBeConvertedToForeach
//                for (int i = 0; i < memberList.Count; ++i)
//                {
//                    int ndx = m_members.FindIndex(_member => _member.ID == memberList[i].ID);
//                    PartyMember member = m_members[ndx];
//                    member.SetPercentHealth(memberList[i].PercentHealth);
//                    m_members[ndx] = member;
//                }
//            }
//        }

        //private void _addRemoveButtonForMember(PartyMember member)
        //{
        //    int delta = m_removeTexture.Height / 3;
        //    bool enabled = m_mainIsLeader || member.ID == OldWorld.Instance.MainPlayer.ActiveCharacter.ID;
        //    XNAButton nextButton = new XNAButton(m_removeTexture,
        //        new Vector2(DrawAreaWithOffset.X + DRAW_REMOVE_X, DRAW_OFFSET_Y),
        //        enabled ? new Rectangle(0, 0, m_removeTexture.Width, delta) : new Rectangle(0, delta, m_removeTexture.Width, delta),
        //        enabled ? new Rectangle(0, delta * 2, m_removeTexture.Width, delta) : new Rectangle(0, delta, m_removeTexture.Width, delta));
        //    if (enabled)
        //    {
        //        PartyMember localMember = member;
        //        nextButton.OnClick += (sender, args) => RemoveMember(localMember.ID);
        //    }
        //    nextButton.SetParent(this);
        //    m_buttons.Add(nextButton);
        //}

        //public void AddMember(PartyMember member)
        //{
        //    m_members.Add(member);

        //    m_numMembers.Text = $"{m_members.Count}";
        //    m_scrollBar.UpdateDimensions(m_members.Count);

        //    _addRemoveButtonForMember(member);

        //    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, member.Name, EOResourceID.STATUS_LABEL_PARTY_JOINED_YOUR);
        //    ((EOGame)Game).Hud.AddChat(ChatTab.System, "", member.Name + " " + OldWorld.GetString(EOResourceID.STATUS_LABEL_PARTY_JOINED_YOUR), ChatIcon.PlayerParty, ChatColor.PM);
        //}

        //public void RemoveMember(short memberID)
        //{
        //    int memberIndex = m_members.FindIndex(_member => _member.ID == memberID);
        //    if (memberIndex < 0 || memberIndex >= m_members.Count)
        //        return;

        //    //if (!((EOGame) Game).API.PartyRemovePlayer(m_members[memberIndex].ID))
        //        ((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();

        //    string name = m_members[memberIndex].Name;

        //    m_members.RemoveAt(memberIndex);
        //    m_buttons[memberIndex].SetParent(null);
        //    m_buttons[memberIndex].Close();
        //    m_buttons.RemoveAt(memberIndex);

        //    m_numMembers.Text = "" + m_members.Count;
        //    m_scrollBar.UpdateDimensions(m_members.Count);
        //    if (m_members.Count <= m_scrollBar.LinesToRender)
        //        m_scrollBar.ScrollToTop();

        //    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, name, EOResourceID.STATUS_LABEL_PARTY_LEFT_YOUR);
        //    //((EOGame)Game).Hud.AddChat(ChatTab.System, "", name + " " + OldWorld.GetString(EOResourceID.STATUS_LABEL_PARTY_LEFT_YOUR), ChatIcon.PlayerPartyDark, ChatColor.PM);
        //}

        //public void CloseParty()
        //{
        //    m_members.Clear();
        //    Visible = false;
        //    m_numMembers.Text = "0";
        //    m_scrollBar.UpdateDimensions(0);
        //}

        //public bool PlayerIsMember(short ID)
        //{
        //    return m_members != null && m_members.FindIndex(_member => _member.ID == ID) >= 0;
        //}

        public override void Update(GameTime gameTime)
        {
            if (!Visible && m_buttons.Any(_btn => _btn.Visible))
            {
                m_buttons.ForEach(_btn => _btn.Visible = false);
                return;
            }

            for (int i = 0; i < m_buttons.Count; ++i)
            {
                if (i < m_scrollBar.ScrollOffset || i >= m_scrollBar.ScrollOffset + m_scrollBar.LinesToRender)
                    m_buttons[i].Visible = false;
                else
                    m_buttons[i].Visible = true;
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
            {
                base.Draw(gameTime);
                return;
            }

            //if(m_members != null)
            //{
            //    SpriteBatch.Begin();
            //    for (int i = m_scrollBar.ScrollOffset; i < m_scrollBar.LinesToRender + m_scrollBar.ScrollOffset && i < m_members.Count; ++i)
            //    {
                    //PartyMember member = m_members[i];
                    //int yCoord = DRAW_OFFSET_Y + DrawAreaWithOffset.Y + (i - m_scrollBar.ScrollOffset) * 13;
                    //m_buttons[i].DrawLocation = new Vector2(DRAW_REMOVE_X, yCoord - DrawAreaWithOffset.Y + 1);
                    //SpriteBatch.Draw(OldChatTab.GetChatIcon(member.IsLeader ? ChatIcon.Star : ChatIcon.Player), new Vector2(DrawAreaWithOffset.X + DRAW_ICON_X, yCoord), Color.White);
                    //SpriteBatch.DrawString(((EOGame) Game).DBGFont, member.Name, new Vector2(DrawAreaWithOffset.X + DRAW_NAME_X, yCoord), Color.Black);
                    //SpriteBatch.DrawString(((EOGame) Game).DBGFont, "" + member.Level, new Vector2(DrawAreaWithOffset.X + DRAW_LEVEL_X, yCoord), Color.Black);
                    //SpriteBatch.DrawString(((EOGame) Game).DBGFont, "HP", new Vector2(DrawAreaWithOffset.X + DRAW_HP_X, yCoord), Color.Black);
                    //_drawHealthBar(member.PercentHealth, yCoord);
            //    }
            //    SpriteBatch.End();
            //}

            base.Draw(gameTime);
        }

        private void _drawHealthBar(int percentHealth, int yCoord)
        {
            yCoord += 1; //slightly offset from the rest of the row
            Rectangle barSrcRect = new Rectangle(0, 0, (int)Math.Round(m_healthBar[HP_RED].Width * (percentHealth / 100.0)), m_healthBar[1].Height);
            SpriteBatch.Draw(m_healthBar[HP_OUTLINE], new Vector2(DrawAreaWithOffset.X + DRAW_HEALTHBAR_X, yCoord), Color.White);

            int color = percentHealth > 50 ? HP_GREEN : percentHealth > 25 ? HP_YELLOW : HP_RED;
            SpriteBatch.Draw(m_healthBar[color], new Vector2(DrawAreaWithOffset.X + DRAW_HEALTHBAR_X, yCoord), barSrcRect, Color.White);
        }

        protected override void OnVisibleChanged(object sender, EventArgs args)
        {
            //if (Visible && m_members.Count > 0 && !((EOGame) Game).API.PartyListMembers())
                ((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();

            base.OnVisibleChanged(sender, args);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Texture2D t in m_healthBar)
                    t.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
