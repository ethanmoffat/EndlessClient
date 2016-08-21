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
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    //todo: maybe some of this can be shared between ChatPanel and NewsPanel
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
        private ChatTab _currentTab;
        private bool _privateChat1Shown, _privateChat2Shown;

        private readonly ISpriteSheet _smallSelected, _smallUnselected;
        private readonly ISpriteSheet _largeSelected, _largeUnselected;

        private readonly Dictionary<ChatTab, XNALabel> _tabLabels;

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
            _currentTab = ChatTab.Local;

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
        }

        public override void Update(GameTime gameTime)
        {
            if (_scrollBar == null)
                return;

            var chatChanged = false;
            if (!_cachedChatData.SequenceEqual(_chatProvider.AllChat[_currentTab]))
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

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible) return;

            base.Draw(gameTime);

            if (_chatRenderables == null)
                return;

            foreach (var renderable in _chatRenderables)
                renderable.Render(SpriteBatch, _chatFont, _nativeGraphicsManager);

            DrawTabsAtBottom();
            foreach (var tabLabel in _tabLabels.Values)
                tabLabel.Draw(gameTime);
        }

        private void UpdateCachedChatData()
        {
            _cachedChatData.Clear();
            _cachedChatData.AddRange(_chatProvider.AllChat[_currentTab]);
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
                //todo: handling for PM (empty sprite sheet if not open!)
                case ChatTab.Private1:
                case ChatTab.Private2:
                    return _currentTab == tab ? _largeSelected : _largeUnselected;
                case ChatTab.Local:
                case ChatTab.Global:
                case ChatTab.Group:
                case ChatTab.System:
                    return _currentTab == tab ? _smallSelected : _smallUnselected;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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