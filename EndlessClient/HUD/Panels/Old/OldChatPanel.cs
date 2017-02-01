// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Globalization;
using EndlessClient.Old;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;
using XNAControl = XNAControls.Old.XNAControl;
using XNALabel = XNAControls.Old.XNALabel;

namespace EndlessClient.HUD.Panels.Old
{
    public class OldChatTab : XNAControl
    {
        private bool Selected { get; }
        private ChatTab WhichTab { get; }
        private readonly OldScrollBar scrollBar;

        private struct ChatIndex : IComparable
        {
            /// <summary>
            /// Used for sorting the entries in the chat window
            /// </summary>
            private readonly int _index;
            /// <summary>
            /// Determines the type of special icon that should appear next to the chat message
            /// </summary>
            public readonly ChatIcon Icon;
            /// <summary>
            /// The entity that talked
            /// </summary>
            public readonly string Who;

            private readonly ChatColor col;

            public ChatIndex(int index = 0, ChatIcon icon = ChatIcon.None, string who = "", ChatColor color = ChatColor.Default)
            {
                _index = index;
                Icon = icon;
                if (who != null && who.Length >= 1)
                {
                    //first character of the who string is capitalized: always!
                    who = who.Substring(0, 1).ToUpper() + who.Substring(1).ToLower();
                }
                Who = who;
                col = color;
            }

            public int CompareTo(object other)
            {
                ChatIndex obj = (ChatIndex)other;
                return _index - obj._index;
            }

            public Color GetColor()
            {
                switch(col)
                {
                    case ChatColor.Default: return Color.Black;
                    case ChatColor.Error: return Color.FromNonPremultiplied(0x7d, 0x0a, 0x0a, 0xff);
                    case ChatColor.PM: return Color.FromNonPremultiplied(0x5a, 0x3c, 0x00, 0xff);
                    case ChatColor.Server: return Color.FromNonPremultiplied(0xe6, 0xd2, 0xc8, 0xff);
                    case ChatColor.ServerGlobal: return ColorConstants.LightYellowText;
                    case ChatColor.Admin: return Color.FromNonPremultiplied(0xc8, 0xaa, 0x96, 0xff);
                    default: throw new IndexOutOfRangeException("ChatColor enumeration unhandled for index " + _index.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private readonly SortedList<ChatIndex, string> chatStrings = new SortedList<ChatIndex, string>();

        //this is here so we don't add more than one chat string at a time.
        private static readonly object ChatStringsLock = new object();

        private readonly XNALabel tabLabel;

        private Vector2 relativeTextPos;

        /// <summary>
        /// This Constructor should be used for all values in ChatTabs
        /// </summary>
        public OldChatTab(ChatTab tab, OldChatRenderer parentRenderer, bool selected = false)
            : base(null, null, parentRenderer)
        {
            WhichTab = tab;
            
            tabLabel = new XNALabel(new Rectangle(14, 2, 1, 1), Constants.FontSize08);
            tabLabel.SetParent(this);

            switch(WhichTab)
            {
                case ChatTab.Local: tabLabel.Text = "scr";  break;
                case ChatTab.Global: tabLabel.Text = "glb"; break;
                case ChatTab.Group: tabLabel.Text = "grp"; break;
                case ChatTab.System: tabLabel.Text = "sys"; break;
                case ChatTab.Private1:
                case ChatTab.Private2:
                    tabLabel.Text = "[priv " + ((int)WhichTab + 1) + "]";
                    break;
            }
            Selected = selected;

            relativeTextPos = new Vector2(20, 3);
            
            //568 331
            scrollBar = new OldScrollBar(parent, new Vector2(467, 2), new Vector2(16, 97), ScrollBarColors.LightOnMed)
            {
                Visible = selected,
                LinesToRender = 7
            };
            OldWorld.IgnoreDialogs(scrollBar);
        }

        /// <summary>
        /// Adds text to the tab. For multi-line text strings, does text wrapping. For text length > 415 pixels, does text wrapping
        /// </summary>
        /// <param name="who">Person that spoke</param>
        /// <param name="text">Message that was spoken</param>
        /// <param name="icon">Icon to display next to the chat</param>
        /// <param name="col">Rendering color (enumerated value)</param>
        public void AddText(string who, string text, ChatIcon icon = ChatIcon.None, ChatColor col = ChatColor.Default)
        {
            const int LINE_LEN = 380;

            //special case: blank line, like in the news panel between news items
            if (string.IsNullOrWhiteSpace(who) && string.IsNullOrWhiteSpace(text))
            {
                lock(ChatStringsLock)
                    chatStrings.Add(new ChatIndex(chatStrings.Count, icon, who, col), " ");
                scrollBar.UpdateDimensions(chatStrings.Count);
                if (chatStrings.Count > 7)
                {
                    scrollBar.ScrollToEnd();
                }
                if (!Selected)
                    tabLabel.ForeColor = Color.White;
                if (!Visible)
                    Visible = true;
                return;
            }

            string whoPadding = "  "; //padding string for additional lines if it is a multi-line message
            if (!string.IsNullOrEmpty(who))
                while (EOGame.Instance.DBGFont.MeasureString(whoPadding).X < EOGame.Instance.DBGFont.MeasureString(who).X)
                    whoPadding += " ";

            TextSplitter ts = new TextSplitter(text, EOGame.Instance.DBGFont)
            {
                LineLength = LINE_LEN,
                LineEnd = "",
                LineIndent = whoPadding
            };
            
            if (!ts.NeedsProcessing)
            {
                lock (ChatStringsLock)
                    chatStrings.Add(new ChatIndex(chatStrings.Count, icon, who, col), text);
            }
            else
            {
                List<string> chatStringsToAdd = ts.SplitIntoLines();

                for (int i = 0; i < chatStringsToAdd.Count; ++i)
                {
                    lock (ChatStringsLock)
                    {
                        if (i == 0)
                            chatStrings.Add(new ChatIndex(chatStrings.Count, icon, who, col), chatStringsToAdd[0]);
                        else
                            chatStrings.Add(new ChatIndex(chatStrings.Count, ChatIcon.None, "", col), chatStringsToAdd[i]);
                    }
                }
            }

            scrollBar.UpdateDimensions(chatStrings.Count);
            if (chatStrings.Count > 7)
            {
                scrollBar.ScrollToEnd();
            }
            if (!Selected)
                tabLabel.ForeColor = Color.White;
            if (!Visible)
                Visible = true;
        }

        public static Texture2D GetChatIcon(ChatIcon icon)
        {
            Texture2D ret = new Texture2D(EOGame.Instance.GraphicsDevice, 13, 13);
            if (icon == ChatIcon.None)
                return ret;

            Color[] data = new Color[169]; //each icon is 13x13
            EOGame.Instance.GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true).GetData(0, new Rectangle(0, (int)icon * 13, 13, 13), data, 0, 169);
            ret.SetData(data);

            return ret;
        }

        public override void Draw(GameTime gameTime)
        {
            if (Selected) //only draw this tab if it is selected
            {
                if (scrollBar == null) return; //prevent nullreferenceexceptions

                //draw icons for the text strings based on the icon specified in the chatStrings Key of the pair
                for (int i = scrollBar.ScrollOffset; i < scrollBar.ScrollOffset + scrollBar.LinesToRender; ++i) //draw 7 lines
                {
                    if (i >= chatStrings.Count)
                        break;

                    SpriteBatch.Begin();
                    Vector2 pos = new Vector2(parent.DrawAreaWithOffset.X, parent.DrawAreaWithOffset.Y + relativeTextPos.Y + (i - scrollBar.ScrollOffset)*13);
                    SpriteBatch.Draw(GetChatIcon(chatStrings.Keys[i].Icon), new Vector2(pos.X + 3, pos.Y), Color.White);

                    string strToDraw;
                    if (string.IsNullOrEmpty(chatStrings.Keys[i].Who))
                        strToDraw = chatStrings.Values[i];
                    else
                        strToDraw = chatStrings.Keys[i].Who + "  " + chatStrings.Values[i];

                    SpriteBatch.DrawString(EOGame.Instance.DBGFont, strToDraw, new Vector2(pos.X + 20, pos.Y), chatStrings.Keys[i].GetColor());
                    SpriteBatch.End();
                }

            }
            base.Draw(gameTime); //draw child controls though
        }
    }

    /// <summary>
    /// Stores all the different tabs, draws their tab graphics, and handles switching between tabs.
    /// </summary>
    public class OldChatRenderer : XNAControl
    {
        private readonly OldChatTab[] tabs;

        public OldChatRenderer()
        {
            tabs = new OldChatTab[Enum.GetNames(typeof(ChatTab)).Length - 1]; // -1 skips the 'none' tab which is used for news
            for(int i = 0; i < tabs.Length; ++i)
            {
                tabs[i] = new OldChatTab((ChatTab) i, this, (ChatTab) i == ChatTab.Local)
                {
                    DrawLocation = i > (int) ChatTab.Private2
                        ? new Vector2(289 + 44*(i - 2), 102)
                        : new Vector2((ChatTab) i == ChatTab.Private1 ? 23 : 156, 102)
                };
            }
        }

        public void AddTextToTab(ChatTab tab, string who, string text, ChatIcon icon = ChatIcon.None, ChatColor col = ChatColor.Default)
        {
            tabs[(int)tab].AddText(who, text, icon, col);
        }

        public static ChatIcon GetChatTypeFromPaperdollIcon(PaperdollIconType whichIcon)
        {
            ChatIcon icon;
            switch (whichIcon)
            {
                case PaperdollIconType.Normal:
                    icon = ChatIcon.Player;
                    break;
                case PaperdollIconType.GM:
                    icon = ChatIcon.GM;
                    break;
                case PaperdollIconType.HGM:
                    icon = ChatIcon.HGM;
                    break;
                case PaperdollIconType.Party:
                    icon = ChatIcon.PlayerParty;
                    break;
                case PaperdollIconType.GMParty:
                    icon = ChatIcon.GMParty;
                    break;
                case PaperdollIconType.HGMParty:
                    icon = ChatIcon.HGMParty;
                    break;
                case PaperdollIconType.SLNBot:
                    icon = ChatIcon.PlayerPartyDark;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(whichIcon), "Invalid Icon type specified.");
            }
            return icon;
        }

        public static string Filter(string text, bool isMainPlayer)
        {
            if (OldWorld.Instance.StrictFilterEnabled || OldWorld.Instance.CurseFilterEnabled)
            {
                foreach (string curse in OldWorld.Instance.DataFiles[DataFiles.CurseFilter].Data.Values)
                {
                    if (string.IsNullOrWhiteSpace(curse))
                        continue;

                    if (text.Contains(curse))
                    {
                        if (OldWorld.Instance.StrictFilterEnabled && isMainPlayer)
                        {
                            EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.YOUR_MIND_PREVENTS_YOU_TO_SAY);
                            return null;
                        }
                        if (OldWorld.Instance.StrictFilterEnabled)
                        {
                            return null;
                        }
                        if (OldWorld.Instance.CurseFilterEnabled)
                        {
                            text = text.Replace(curse, "****");
                        }
                    }
                }
            }
            return text;
        }
    }
}
