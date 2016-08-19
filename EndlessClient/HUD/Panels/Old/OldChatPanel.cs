// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Globalization;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace EndlessClient.HUD.Panels.Old
{
    /// <summary>
    /// contains a scroll bar and handles the text display and storage for a particular tab
    /// </summary>
    public class OldChatTab : XNAControl
    {
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                scrollBar.Visible = _selected;
                tabLabel.ForeColor = _selected ? Color.White : Color.Black;
            }
        }
        public ChatTab WhichTab { get; private set; }

        private Rectangle? closeRect;
        private readonly ScrollBar scrollBar;

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
        public string ChatCharacter
        {
            get { return WhichTab == ChatTab.Private1 || WhichTab == ChatTab.Private2 ? tabLabel.Text : null; }
            set { tabLabel.Text = value; }
        }

        public bool PrivateChatUnused
        {
            get { return tabLabel.Text.Contains("[") || tabLabel.Text == ""; }
        }

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
            _selected = selected;

            relativeTextPos = new Vector2(20, 3);

            //enable close button based on which tab was specified
            switch(WhichTab)
            {
                case ChatTab.Private1:
                case ChatTab.Private2:
                    {
                        closeRect = new Rectangle(3, 3, 11, 11);
                        drawArea = new Rectangle(drawArea.X, drawArea.Y, 132, 16);
                        Visible = false;
                    } break;
                default:
                    {
                        closeRect = null;
                        drawArea = new Rectangle(drawArea.X, drawArea.Y, 43, 16);
                        Visible = true;
                    } break;
            }
            
            //568 331
            scrollBar = new ScrollBar(parent, new Vector2(467, 2), new Vector2(16, 97), ScrollBarColors.LightOnMed)
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

        public void ClosePrivateChat()
        {
            Visible = Selected = false;
            tabLabel.Text = "";
            lock (ChatStringsLock)
                chatStrings.Clear();
            ((OldChatRenderer)parent).SetSelectedTab(ChatTab.Local);
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
        
        public override void Update(GameTime gameTime)
        {
            if (!Visible || !EOGame.Instance.IsActive)
                return;

            MouseState mouseState = Mouse.GetState();
            //this is our own button press handler
            if (MouseOver && mouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                if (!Selected)
                {
                    ((OldChatRenderer)parent).SetSelectedTab(WhichTab);
                }

                //logic for handling the close button (not actually a button, was I high when I made this...?)
                if ((WhichTab == ChatTab.Private1 || WhichTab == ChatTab.Private2) && closeRect != null)
                {
                    Rectangle withOffset = new Rectangle(DrawAreaWithOffset.X + closeRect.Value.X, DrawAreaWithOffset.Y + closeRect.Value.Y, closeRect.Value.Width, closeRect.Value.Height);
                    if (withOffset.ContainsPoint(Mouse.GetState().X, Mouse.GetState().Y))
                    {
                        ClosePrivateChat();
                    }
                }
            }
            else if (Selected && mouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                XNAControl tmpParent = parent.GetParent(); //get the panel containing this tab, the parent is the chatRenderer
                if (tmpParent.DrawAreaWithOffset.Contains(mouseState.X, mouseState.Y))
                {
                    int adjustedY = mouseState.Y - tmpParent.DrawAreaWithOffset.Y;
                    int level = (int)Math.Round(adjustedY / 13.0) - 1;
                    if (level >= 0 && scrollBar.ScrollOffset + level < chatStrings.Count)
                    {
                        EOGame.Instance.Hud.SetChatText("!" + chatStrings.Keys[scrollBar.ScrollOffset + level].Who + " ");
                    }
                }
            }

            base.Update(gameTime);
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
        private int currentSelTab;
        private readonly OldChatTab[] tabs;

        public OldChatTab SelectedTab
        {
            get { return tabs[currentSelTab]; }
        }

        public OldChatRenderer()
        {
            tabs = new OldChatTab[Enum.GetNames(typeof(ChatTab)).Length - 1]; // -1 skips the 'none' tab which is used for news
            for(int i = 0; i < tabs.Length; ++i)
            {
                tabs[i] = new OldChatTab((ChatTab)i, this, (ChatTab)i == ChatTab.Local)
                {
                    DrawLocation = i > (int) ChatTab.Private2 ? new Vector2(289 + 44*(i - 2), 102) : new Vector2((ChatTab) i == ChatTab.Private1 ? 23 : 156, 102)
                };
            }

            currentSelTab = (int)ChatTab.Local;
            tabs[currentSelTab].Selected = true;
        }

        public void SetSelectedTab(ChatTab tabToSelect)
        {
            if ((ChatTab) currentSelTab == ChatTab.Global && tabToSelect != ChatTab.Global)
            {
                OldPacket pkt = new OldPacket(PacketFamily.Global, PacketAction.Close);
                pkt.AddChar((byte) 'n');
                OldWorld.Instance.Client.SendPacket(pkt);
            }
            else if(tabToSelect == ChatTab.Global && (ChatTab)currentSelTab != ChatTab.Global)
            {
                OldPacket pkt = new OldPacket(PacketFamily.Global, PacketAction.Open);
                pkt.AddChar((byte) 'y');
                OldWorld.Instance.Client.SendPacket(pkt);
            }

            tabs[currentSelTab].Selected = false;
            tabs[currentSelTab = (int)tabToSelect].Selected = true;
        }

        public void AddTextToTab(ChatTab tab, string who, string text, ChatIcon icon = ChatIcon.None, ChatColor col = ChatColor.Default)
        {
            tabs[(int)tab].AddText(who, text, icon, col);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.Begin();
            //264 16 43x16
            //307 16 43x16

            //the parent renderer draws all the tabs
            foreach (OldChatTab t in tabs)
            {
                Texture2D drawTexture;
                Color[] data;
                switch (t.WhichTab)
                {
                    case ChatTab.Local: //391 433 need to see if this should be relative to top-left of existing chat panel or absolute from top-left of game screen
                    case ChatTab.Global:
                    case ChatTab.Group:
                    case ChatTab.System:
                        {
                            data = new Color[43 * 16];
                            drawTexture = new Texture2D(Game.GraphicsDevice, 43, 16);
                            ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 35).GetData(0, t.Selected ? new Rectangle(307, 16, 43, 16) : new Rectangle(264, 16, 43, 16), data, 0, data.Length);
                            drawTexture.SetData(data);
                        }
                        break;
                    case ChatTab.Private1:
                    case ChatTab.Private2:
                        {
                            if (t.Visible)
                            {
                                data = new Color[132 * 16];
                                drawTexture = new Texture2D(Game.GraphicsDevice, 132, 16);
                                ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 35).GetData(0, t.Selected ? new Rectangle(132, 16, 132, 16) : new Rectangle(0, 16, 132, 16), data, 0, data.Length);
                                drawTexture.SetData(data);
                            }
                            else
                                drawTexture = null;
                        }
                        break;
                    default:
                        throw new InvalidOperationException("This is not a valid enumeration for WhichTab");
                }
                if(drawTexture != null)
                    SpriteBatch.Draw(drawTexture, new Vector2(t.DrawAreaWithOffset.X, t.DrawAreaWithOffset.Y), Color.White);
            }
            SpriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Returns the first open PM tab for a character, or ChatTabs.None if both PM tabs are in use.
        /// </summary>
        /// <param name="character">Character for the conversation</param>
        /// <returns>ChatTab to which text should be added</returns>
        public ChatTab StartConversation(string character)
        {
            int i;
            if (tabs[i = (int) ChatTab.Private1].PrivateChatUnused || tabs[i].ChatCharacter == character)
            {
                tabs[i].ChatCharacter = character;
                return ChatTab.Private1;
            }

            if (tabs[i = (int) ChatTab.Private2].PrivateChatUnused || tabs[i].ChatCharacter == character)
            {
                tabs[i].ChatCharacter = character;
                return ChatTab.Private2;
            }

            //note: this used to return ChatTabs.None before it was removed
            throw new InvalidOperationException("Unable to start PM!");
        }

        /// <summary>
        /// Closes a private chat conversation (called if the response from the server is "not found" for a character)
        /// </summary>
        /// <param name="character">Character for the conversation</param>
        public void ClosePrivateChat(string character)
        {
            int i;
            if (tabs[i = (int) ChatTab.Private1].ChatCharacter == character)
                tabs[i].ClosePrivateChat();
            else if (tabs[i = (int) ChatTab.Private2].ChatCharacter == character)
                tabs[i].ClosePrivateChat();
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
                    throw new ArgumentOutOfRangeException("whichIcon", "Invalid Icon type specified.");
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
