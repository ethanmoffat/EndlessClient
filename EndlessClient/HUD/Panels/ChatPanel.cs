// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Sprites;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class ChatPanel : XNAPanel, IHudPanel
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IChatRenderableGenerator _chatRenderableGenerator;
        private readonly IChatProvider _chatProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly SpriteFont _chatFont;

        private readonly ScrollBar _scrollBar;
        private readonly List<IChatRenderable> _chatRenderables;

        private readonly ChatPanelStateCache _state;

        private readonly ISpriteSheet _smallSelected, _smallUnselected;
        private readonly ISpriteSheet _largeSelected, _largeUnselected;

        private readonly Dictionary<ChatTab, IXNALabel> _tabLabels;
        private readonly IReadOnlyDictionary<ChatTab, Rectangle> _tabLabelClickableAreas;

        private readonly Rectangle _closeButtonAreaForTab1, _closeButtonAreaForTab2;

        public ChatTab CurrentTab { get; private set; }

        public ChatPanel(INativeGraphicsManager nativeGraphicsManager,
                         IChatRenderableGenerator chatRenderableGenerator,
                         IChatProvider chatProvider,
                         IHudControlProvider hudControlProvider,
                         SpriteFont chatFont)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _chatRenderableGenerator = chatRenderableGenerator;
            _chatProvider = chatProvider;
            _hudControlProvider = hudControlProvider;
            _chatFont = chatFont;

            //abs coordiantes: 568 309
            _scrollBar = new ScrollBar(new Vector2(467, 2), new Vector2(16, 97), ScrollBarColors.LightOnMed, _nativeGraphicsManager)
            {
                LinesToRender = 7,
                Visible = true
            };
            _scrollBar.SetParentControl(this);
            _chatRenderables = new List<IChatRenderable>();

            _state = new ChatPanelStateCache();

            CurrentTab = ChatTab.Local;

            var tabTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 35);
            _smallSelected = new SpriteSheet(tabTexture, new Rectangle(307, 16, 43, 16));
            _smallUnselected = new SpriteSheet(tabTexture, new Rectangle(264, 16, 43, 16));
            _largeSelected = new SpriteSheet(tabTexture, new Rectangle(132, 16, 132, 16));
            _largeUnselected = new SpriteSheet(tabTexture, new Rectangle(0, 16, 132, 16));

            _tabLabels = new Dictionary<ChatTab, IXNALabel>
            {
                {ChatTab.Local, new XNALabel(Constants.FontSize08) {Text = "scr", ForeColor = Color.White}},
                {ChatTab.Global, new XNALabel(Constants.FontSize08) {Text = "glb"}},
                {ChatTab.Group, new XNALabel(Constants.FontSize08) {Text = "grp"}},
                {ChatTab.System, new XNALabel(Constants.FontSize08) {Text = "sys"}},
                {ChatTab.Private1, new XNALabel(Constants.FontSize08) {Text = "[priv 1]", Visible = false}},
                {ChatTab.Private2, new XNALabel(Constants.FontSize08) {Text = "[priv 2]", Visible = false}}
            };

            _tabLabelClickableAreas = new Dictionary<ChatTab, Rectangle>
            {
                {ChatTab.Local, new Rectangle(0, 0, 43, 16).WithPosition(GetDestinationVectorForTab(ChatTab.Local))},
                {ChatTab.Global, new Rectangle(0, 0, 43, 16).WithPosition(GetDestinationVectorForTab(ChatTab.Global))},
                {ChatTab.Group, new Rectangle(0, 0, 43, 16).WithPosition(GetDestinationVectorForTab(ChatTab.Group))},
                {ChatTab.System, new Rectangle(0, 0, 43, 16).WithPosition(GetDestinationVectorForTab(ChatTab.System))},
                {ChatTab.Private1, new Rectangle(0, 0, 132, 16).WithPosition(GetDestinationVectorForTab(ChatTab.Private1))},
                {ChatTab.Private2, new Rectangle(0, 0, 132, 16).WithPosition(GetDestinationVectorForTab(ChatTab.Private2))},
            };

            foreach (var kvp in _tabLabels)
            {
                var startPos = GetDestinationVectorForTab(kvp.Key);
                kvp.Value.DrawPosition = startPos + new Vector2(14, 2);

                //todo: see if these need to be manually drawn or not
                //note: these must be manually drawn so they appear on top of the tab graphics
                //if (Game.Components.Contains(kvp.Value))
                //    Game.Components.Remove(kvp.Value);
            }

            _closeButtonAreaForTab1 = new Rectangle(3, 3, 11, 11).WithPosition(GetDestinationVectorForTab(ChatTab.Private1));
            _closeButtonAreaForTab2 = new Rectangle(3, 3, 11, 11).WithPosition(GetDestinationVectorForTab(ChatTab.Private2));

            BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 28);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);
        }

        public override void Initialize()
        {
            _scrollBar.Initialize();
            foreach (var label in _tabLabels.Values)
                label.Initialize();

            base.Initialize();
        }

        public void TryStartNewPrivateChat(string targetCharacter)
        {
            if (_state.PrivateChat1Shown && _state.PrivateChat2Shown)
                return;

            if (_state.PrivateChat1Shown) //private chat 1 is in use
            {
                _state.PrivateChat2Shown = true;
                SelectTab(ChatTab.Private2);
                _tabLabels[ChatTab.Private2].Text = char.ToUpper(targetCharacter[0]) + targetCharacter.Substring(1);
                ((XNALabel)_tabLabels[ChatTab.Private2]).Visible = true;
            }
            else //no private chats are in use
            {
                _state.PrivateChat1Shown = true;
                SelectTab(ChatTab.Private1);
                _tabLabels[ChatTab.Private1].Text = char.ToUpper(targetCharacter[0]) + targetCharacter.Substring(1);
                ((XNALabel)_tabLabels[ChatTab.Private1]).Visible = true;
            }
        }

        public void ClosePMTab(ChatTab whichTab)
        {
            if (whichTab == ChatTab.Private1)
                _state.PrivateChat1Shown = false;
            else if (whichTab == ChatTab.Private2)
                _state.PrivateChat2Shown = false;
            else
                throw new ArgumentOutOfRangeException("whichTab", whichTab, "whichTab should be Private1 or Private2");

            _state.CachedScrollOffsets[whichTab] = 0;
            _tabLabels[whichTab].Text = "";
            SelectTab(ChatTab.Local);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            if (MouseOver && mouseState.LeftButton == ButtonState.Released &&
                PreviousMouseState.LeftButton == ButtonState.Pressed &&
                _tabLabelClickableAreas.Any(x => x.Value.Contains(mouseState.Position)))
            {
                HandleLeftClickOnTabs(mouseState);
            }
            else if (MouseOver && mouseState.RightButton == ButtonState.Released &&
                     PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                HandleRightClickOnChatText(mouseState);
            }

            HandleTextAddedToOtherTabs();

            var chatChanged = false;
            if (!_state.CachedChatDataCurrentTab.SequenceEqual(_chatProvider.AllChat[CurrentTab]))
            {
                UpdateCachedChatData();
                chatChanged = true;
            }

            if (chatChanged || _state.CachedScrollOffsets[CurrentTab] != _scrollBar.ScrollOffset)
            {
                var renderables = _chatRenderableGenerator.GenerateChatRenderables(_state.CachedChatDataCurrentTab);

                _state.CachedScrollOffsets[CurrentTab] = _scrollBar.ScrollOffset;
                SetupRenderablesFromCachedValues(renderables, chatChanged);
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);

            foreach (var renderable in _chatRenderables)
                renderable.Render(_spriteBatch, _chatFont, _nativeGraphicsManager);

            DrawTabsAtBottom();
            //todo
            //foreach (var tabLabel in _tabLabels.Values)
            //    tabLabel.Draw(gameTime);
        }

        #region Update Helpers

        private void HandleTextAddedToOtherTabs()
        {
            var tabs = (ChatTab[]) Enum.GetValues(typeof(ChatTab));

            foreach (var tab in tabs)
            {
                if (CurrentTab == tab)
                    continue;

                var lineCountForTab = _chatProvider.AllChat[tab].Count;
                if (_state.cachedChatTabLineCounts[tab] != lineCountForTab)
                {
                    _state.cachedChatTabLineCounts[tab] = lineCountForTab;
                    _tabLabels[tab].ForeColor = Color.White;
                }
            }
        }

        private void UpdateCachedChatData()
        {
            _state.CachedChatDataCurrentTab.Clear();
            _state.CachedChatDataCurrentTab.AddRange(_chatProvider.AllChat[CurrentTab]);
        }

        private void SetupRenderablesFromCachedValues(IReadOnlyList<IChatRenderable> renderables, bool newText)
        {
            _chatRenderables.Clear();

            //only render based on what the scroll bar's position is
            _chatRenderables.AddRange(renderables.Skip(_scrollBar.ScrollOffset).Take(_scrollBar.LinesToRender));
            for (int i = 0; i < _chatRenderables.Count; ++i)
                _chatRenderables[i].SetDisplayIndex(i);

            //update scrollbar with total number of renderables
            _scrollBar.UpdateDimensions(renderables.Count);

            if (newText && renderables.Count > _scrollBar.LinesToRender)
                _scrollBar.ScrollToEnd();
        }

        private void HandleLeftClickOnTabs(MouseState mouseState)
        {
            var clickedTab = _tabLabelClickableAreas.Single(x => x.Value.Contains(mouseState.Position)).Key;

            //prevent clicking invisible tabs (boolean logic reduced using de morgan's laws)
            if ((clickedTab != ChatTab.Private1 || _state.PrivateChat1Shown) &&
                (clickedTab != ChatTab.Private2 || _state.PrivateChat2Shown))
            {
                if (_closeButtonAreaForTab1.ContainsPoint(mouseState.X, mouseState.Y))
                {
                    ClosePMTab(ChatTab.Private1);
                    SelectTab(ChatTab.Local);
                }
                else if (_closeButtonAreaForTab2.ContainsPoint(mouseState.X, mouseState.Y))
                {
                    ClosePMTab(ChatTab.Private2);
                    SelectTab(ChatTab.Local);
                }
                else
                    SelectTab(clickedTab);
            }
        }

        private void HandleRightClickOnChatText(MouseState mouseState)
        {
            var clickedYRelativeToTopOfPanel = mouseState.Y - DrawAreaWithParentOffset.Y;
            var clickedChatRow = (int)Math.Round(clickedYRelativeToTopOfPanel / 13.0) - 1;

            if (clickedChatRow >= 0 &&
                _scrollBar.ScrollOffset + clickedChatRow < _chatProvider.AllChat[CurrentTab].Count)
            {
                var who = _chatProvider.AllChat[CurrentTab][_scrollBar.ScrollOffset + clickedChatRow].Who;
                _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox).Text =
                    string.Format("!{0} ", who);
            }
        }

        #endregion

        #region Draw Helpers

        private void DrawTabsAtBottom()
        {
            _spriteBatch.Begin();

            var chatTabs = (ChatTab[])Enum.GetValues(typeof(ChatTab));
            foreach (var tab in chatTabs)
            {
                var destVector = GetDestinationVectorForTab(tab);
                var spriteSheet = GetSpriteSheetForTab(tab);
                if (!spriteSheet.HasTexture)
                    continue;

                _spriteBatch.Draw(spriteSheet.SheetTexture,
                                  destVector,
                                  spriteSheet.SourceRectangle,
                                  Color.White);
            }

            _spriteBatch.End();
        }

        private Vector2 GetDestinationVectorForTab(ChatTab tab)
        {
            var topLeft = new Vector2(DrawAreaWithParentOffset.X, DrawAreaWithParentOffset.Y);
            switch (tab)
            {
                case ChatTab.Private1:
                    return topLeft + new Vector2(23, 102);
                case ChatTab.Private2:
                    return topLeft + new Vector2(156, 102);
                case ChatTab.Local:
                case ChatTab.Global:
                case ChatTab.Group:
                case ChatTab.System:
                    return topLeft + new Vector2(289 + 44 * ((int)tab - 2), 102);
                default: throw new ArgumentOutOfRangeException("tab", tab, null);
            }
        }

        private ISpriteSheet GetSpriteSheetForTab(ChatTab tab)
        {
            if ((tab == ChatTab.Private1 && !_state.PrivateChat1Shown) ||
                (tab == ChatTab.Private2 && !_state.PrivateChat2Shown))
                return new EmptySpriteSheet();

            switch (tab)
            {
                case ChatTab.Private1:
                case ChatTab.Private2:
                    return CurrentTab == tab ? _largeSelected : _largeUnselected;
                case ChatTab.Local:
                case ChatTab.Global:
                case ChatTab.Group:
                case ChatTab.System:
                    return CurrentTab == tab ? _smallSelected : _smallUnselected;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        private void SelectTab(ChatTab clickedTab)
        {
            _tabLabels[CurrentTab].ForeColor = Color.Black;
            _tabLabels[clickedTab].ForeColor = Color.White;
            CurrentTab = clickedTab;

            _scrollBar.SetScrollOffset(_state.CachedScrollOffsets[CurrentTab]);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var tabLabel in _tabLabels.Values)
                    tabLabel.Dispose();
                _tabLabels.Clear();
            }
            base.Dispose(disposing);
        }

        private class ChatPanelStateCache
        {
            private readonly List<ChatData> _cachedChatDataCurrentTab;
            private readonly Dictionary<ChatTab, int> _cachedChatTabLineCounts;
            private readonly Dictionary<ChatTab, int> _cachedScrollOffsets;

            public List<ChatData> CachedChatDataCurrentTab { get { return _cachedChatDataCurrentTab; } }
            public Dictionary<ChatTab, int> cachedChatTabLineCounts { get { return _cachedChatTabLineCounts; } }

            public Dictionary<ChatTab, int> CachedScrollOffsets { get { return _cachedScrollOffsets; } }

            public bool PrivateChat1Shown { get; set; }
            public bool PrivateChat2Shown { get; set; }

            internal ChatPanelStateCache()
            {
                var chatTabs = (ChatTab[])Enum.GetValues(typeof(ChatTab));

                _cachedChatDataCurrentTab = new List<ChatData>();
                _cachedChatTabLineCounts = chatTabs.ToDictionary(k => k, v => 0);
                _cachedChatTabLineCounts[ChatTab.Local] = 1; //1 line of default news text
                _cachedChatTabLineCounts[ChatTab.Global] = 2; //2 lines default text

                _cachedScrollOffsets = chatTabs.ToDictionary(k => k, v => 0);
            }
        }
    }
}