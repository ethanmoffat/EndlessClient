// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly SpriteFont _chatFont;

        private readonly ScrollBar _scrollBar;
        private readonly List<IChatRenderable> _chatRenderables;

        private readonly List<ChatData> _cachedChatData;
        private int _cachedScrollOffset;
        private int _cachedLinesToRender;
        private bool _privateChat1Shown, _privateChat2Shown;

        private readonly ISpriteSheet _smallSelected, _smallUnselected;
        private readonly ISpriteSheet _largeSelected, _largeUnselected;

        private readonly Dictionary<ChatTab, XNALabel> _tabLabels;
        private readonly IReadOnlyDictionary<ChatTab, Rectangle> _tabLabelClickableAreas;

        private readonly bool _constructed;

        public ChatTab CurrentTab { get; private set; }

        public ChatPanel(INativeGraphicsManager nativeGraphicsManager,
                         IChatRenderableGenerator chatRenderableGenerator,
                         IChatProvider chatProvider,
                         SpriteFont chatFont)
            : base(new Rectangle(102, 330, 1, 1))
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _chatRenderableGenerator = chatRenderableGenerator;
            _chatProvider = chatProvider;
            _chatFont = chatFont;

            //abs coordiantes: 568 309
            _scrollBar = new ScrollBar(this, new Vector2(467, 2), new Vector2(16, 97), ScrollBarColors.LightOnMed, _nativeGraphicsManager)
            {
                LinesToRender = 7,
                Visible = true
            };
            _chatRenderables = new List<IChatRenderable>();

            _cachedChatData = new List<ChatData>();
            _cachedScrollOffset = -1;
            _cachedLinesToRender = -1;
            CurrentTab = ChatTab.Local;

            var tabTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 35);
            _smallSelected = new SpriteSheet(tabTexture, new Rectangle(307, 16, 43, 16));
            _smallUnselected = new SpriteSheet(tabTexture, new Rectangle(264, 16, 43, 16));
            _largeSelected = new SpriteSheet(tabTexture, new Rectangle(132, 16, 132, 16));
            _largeUnselected = new SpriteSheet(tabTexture, new Rectangle(0, 16, 132, 16));

            _tabLabels = new Dictionary<ChatTab, XNALabel>
            {
                {ChatTab.Local, new XNALabel(new Rectangle(1, 1, 1, 1), Constants.FontSize08) {Text = "scr", ForeColor = Color.White}},
                {ChatTab.Global, new XNALabel(new Rectangle(1, 1, 1, 1), Constants.FontSize08) {Text = "glb"}},
                {ChatTab.Group, new XNALabel(new Rectangle(1, 1, 1, 1), Constants.FontSize08) {Text = "grp"}},
                {ChatTab.System, new XNALabel(new Rectangle(1, 1, 1, 1), Constants.FontSize08) {Text = "sys"}},
                {ChatTab.Private1, new XNALabel(new Rectangle(1, 1, 1, 1), Constants.FontSize08) {Text = "[priv 1]", Visible = false}},
                {ChatTab.Private2, new XNALabel(new Rectangle(1, 1, 1, 1), Constants.FontSize08) {Text = "[priv 2]", Visible = false}}
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
                kvp.Value.DrawLocation = startPos + new Vector2(14, 2);

                //note: these must be manually drawn so they appear on top of the tab graphics
                if (Game.Components.Contains(kvp.Value))
                    Game.Components.Remove(kvp.Value);
            }

            BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 28);
            _setSize(BackgroundImage.Width, BackgroundImage.Height);

            _constructed = true;
        }

        public void TryStartNewPrivateChat(string targetCharacter)
        {
            if (_privateChat1Shown && _privateChat2Shown)
                return;

            if (_privateChat1Shown) //private chat 1 is in use
            {
                _privateChat2Shown = true;
                SelectTab(ChatTab.Private2);
                _tabLabels[ChatTab.Private2].Text = char.ToUpper(targetCharacter[0]) + targetCharacter.Substring(1);
                _tabLabels[ChatTab.Private2].Visible = true;
            }
            else //no private chats are in use
            {
                _privateChat1Shown = true;
                SelectTab(ChatTab.Private1);
                _tabLabels[ChatTab.Private1].Text = char.ToUpper(targetCharacter[0]) + targetCharacter.Substring(1);
                _tabLabels[ChatTab.Private1].Visible = true;
            }
        }

        public void ClosePrivateChat(ChatTab whichTab)
        {
            if (whichTab == ChatTab.Private1)
                _privateChat1Shown = false;
            else if (whichTab == ChatTab.Private2)
                _privateChat2Shown = false;
            else
                throw new ArgumentOutOfRangeException("whichTab");

            _tabLabels[whichTab].Text = "";
            SelectTab(ChatTab.Local);
        }

        public override void Update(GameTime gameTime)
        {
            if (!_constructed)
                return;

            //todo: some sort of change detection for when text is added to tabs
            var chatChanged = false;
            if (!_cachedChatData.SequenceEqual(_chatProvider.AllChat[CurrentTab]))
            {
                UpdateCachedChatData();
                chatChanged = true;
            }

            if (chatChanged ||
                _cachedScrollOffset != _scrollBar.ScrollOffset ||
                _cachedLinesToRender != _scrollBar.LinesToRender)
            {
                var renderables = _chatRenderableGenerator.GenerateChatRenderables(_cachedChatData);

                UpdateCachedScrollProperties();
                SetupRenderablesFromCachedValues(renderables);
            }

            var mouseState = Mouse.GetState();
            if (MouseOver && mouseState.LeftButton == ButtonState.Released &&
                PreviousMouseState.LeftButton == ButtonState.Pressed &&
                _tabLabelClickableAreas.Any(x => x.Value.Contains(mouseState.Position))) //handle left click
            {
                var clickedTab = _tabLabelClickableAreas.Single(x => x.Value.Contains(mouseState.Position)).Key;
                
                //prevent clicking invisible tabs (boolean logic reduced using de morgan's laws
                if ((clickedTab != ChatTab.Private1 || _privateChat1Shown) &&
                    (clickedTab != ChatTab.Private2 || _privateChat2Shown))
                {
                    //todo: special-case handling for close buttons
                    SelectTab(clickedTab);
                }
            }
            else if (MouseOver && mouseState.RightButton == ButtonState.Released &&
                     PreviousMouseState.RightButton == ButtonState.Pressed) //handle right click
            {
                //todo: start private chat with the person that said whatever line was right-clicked
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible || !_constructed) return;

            base.Draw(gameTime);

            foreach (var renderable in _chatRenderables)
                renderable.Render(SpriteBatch, _chatFont, _nativeGraphicsManager);

            DrawTabsAtBottom();
            foreach (var tabLabel in _tabLabels.Values)
                tabLabel.Draw(gameTime);
        }

        private void UpdateCachedChatData()
        {
            _cachedChatData.Clear();
            _cachedChatData.AddRange(_chatProvider.AllChat[CurrentTab]);
        }

        private void UpdateCachedScrollProperties()
        {
            _cachedScrollOffset = _scrollBar.ScrollOffset;
            _cachedLinesToRender = _scrollBar.LinesToRender;
        }

        private void SetupRenderablesFromCachedValues(IReadOnlyList<IChatRenderable> renderables)
        {
            _chatRenderables.Clear();

            //only render based on what the scroll bar's position is
            _chatRenderables.AddRange(renderables.Skip(_cachedScrollOffset).Take(_cachedLinesToRender));
            for (int i = 0; i < _chatRenderables.Count; ++i)
                _chatRenderables[i].SetDisplayIndex(i);

            //update scrollbar with total number of renderables
            _scrollBar.UpdateDimensions(renderables.Count);

            if (renderables.Count > _scrollBar.LinesToRender)
                _scrollBar.ScrollToEnd();
        }

        private void DrawTabsAtBottom()
        {
            SpriteBatch.Begin();

            var chatTabs = (ChatTab[])Enum.GetValues(typeof(ChatTab));
            foreach (var tab in chatTabs)
            {
                var destVector = GetDestinationVectorForTab(tab);
                var spriteSheet = GetSpriteSheetForTab(tab);
                if (!spriteSheet.HasTexture)
                    continue;

                SpriteBatch.Draw(spriteSheet.SheetTexture,
                                 destVector,
                                 spriteSheet.SourceRectangle,
                                 Color.White);
            }

            SpriteBatch.End();
        }

        private Vector2 GetDestinationVectorForTab(ChatTab tab)
        {
            var topLeft = new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y);
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
            if ((tab == ChatTab.Private1 && !_privateChat1Shown) ||
                (tab == ChatTab.Private2 && !_privateChat2Shown))
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

        private void SelectTab(ChatTab clickedTab)
        {
            _tabLabels[CurrentTab].ForeColor = Color.Black;
            _tabLabels[clickedTab].ForeColor = Color.White;
            CurrentTab = clickedTab;
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
    }
}