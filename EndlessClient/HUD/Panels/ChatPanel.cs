// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EndlessClient.Rendering.Chat;
using EndlessClient.UIControls;
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
    }
}