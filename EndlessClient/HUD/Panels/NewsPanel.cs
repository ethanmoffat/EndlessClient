using EndlessClient.Rendering;
using EndlessClient.Rendering.Chat;
using EndlessClient.UIControls;
using EOLib.Domain.Login;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class NewsPanel : DraggableHudPanel
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IChatRenderableGenerator _chatRenderableGenerator;
        private readonly INewsProvider _newsProvider;
        private readonly ScrollBar _scrollBar;
        private readonly List<IChatRenderable> _chatRenderables;
        private readonly BitmapFont _chatFont;

        //cached data fields
        private readonly List<string> _cachedNewsStrings;
        private int _cachedScrollOffset;
        private int _cachedLinesToRender;

        private bool _firstTime = true;

        public NewsPanel(INativeGraphicsManager nativeGraphicsManager,
                         IChatRenderableGenerator chatRenderableGenerator,
                         INewsProvider newsProvider,
                         BitmapFont chatFont,
                         IClientWindowSizeProvider clientWindowSizeProvider)
            : base(clientWindowSizeProvider.Resizable)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _chatRenderableGenerator = chatRenderableGenerator;
            _newsProvider = newsProvider;

            //abs coordiantes: 568 331
            _scrollBar = new ScrollBar(new Vector2(467, 20), new Vector2(16, 97), ScrollBarColors.LightOnMed, _nativeGraphicsManager)
            {
                LinesToRender = 7,
                Visible = true
            };
            _scrollBar.SetParentControl(this);
            SetScrollWheelHandler(_scrollBar);

            _chatRenderables = new List<IChatRenderable>();
            _chatFont = chatFont;

            _cachedNewsStrings = new List<string>();
            _cachedScrollOffset = -1;
            _cachedLinesToRender = -1;

            BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 48);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);
        }

        public override void Initialize()
        {
            _scrollBar.Initialize();
            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            var newsChanged = false;
            if (!_cachedNewsStrings.SequenceEqual(_newsProvider.NewsText))
            {
                UpdateCachedNewsStrings();
                newsChanged = true;
            }

            if (newsChanged ||
                _cachedScrollOffset != _scrollBar.ScrollOffset ||
                _cachedLinesToRender != _scrollBar.LinesToRender)
            {
                var renderables = _chatRenderableGenerator.GenerateNewsRenderables(_cachedNewsStrings);

                UpdateCachedScrollProperties();
                SetupRenderablesFromCachedValues(renderables);

                if (_firstTime && renderables.Count > _scrollBar.LinesToRender)
                {
                    _scrollBar.SetDownArrowFlashSpeed(500);
                    _firstTime = false;
                }
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);

            if (_chatRenderables == null)
                return;

            foreach (var renderable in _chatRenderables)
                renderable.Render(this, _spriteBatch, _chatFont);
        }

        private void UpdateCachedNewsStrings()
        {
            _cachedNewsStrings.Clear();
            _cachedNewsStrings.AddRange(_newsProvider.NewsText);
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
                _chatRenderables[i].DisplayIndex = i;

            //update scrollbar with total number of renderables
            _scrollBar.UpdateDimensions(renderables.Count);
        }
    }
}