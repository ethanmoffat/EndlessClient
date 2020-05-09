using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Character
{
    public class CharacterRenderer : DrawableGameComponent, ICharacterRenderer
    {
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly ICharacterProvider _characterProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly ICharacterPropertyRendererBuilder _characterPropertyRendererBuilder;
        private readonly ICharacterTextures _characterTextures;
        private readonly ICharacterSpriteCalculator _characterSpriteCalculator;
        private readonly IGameStateProvider _gameStateProvider;
        private ICharacterRenderProperties _renderProperties;
        private bool _textureUpdateRequired, _positionIsRelative = true;

        private SpriteBatch _sb;
        private RenderTarget2D _charRenderTarget;
        private Texture2D _outline;

        public ICharacterRenderProperties RenderProperties
        {
            get { return _renderProperties; }
            set
            {
                if (_renderProperties == value) return;
                _renderProperties = value;
                _textureUpdateRequired = true;
            }
        }

        public Rectangle DrawArea { get; private set; }

        public int? TopPixel { get; private set; }

        public CharacterRenderer(Game game,
                                 IRenderTargetFactory renderTargetFactory,
                                 ICharacterProvider characterProvider,
                                 IRenderOffsetCalculator renderOffsetCalculator,
                                 ICharacterPropertyRendererBuilder characterPropertyRendererBuilder,
                                 ICharacterTextures characterTextures,
                                 ICharacterSpriteCalculator characterSpriteCalculator,
                                 ICharacterRenderProperties renderProperties,
                                 IGameStateProvider gameStateProvider)
            : base(game)
        {
            _renderTargetFactory = renderTargetFactory;
            _characterProvider = characterProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
            _characterPropertyRendererBuilder = characterPropertyRendererBuilder;
            _characterTextures = characterTextures;
            _characterSpriteCalculator = characterSpriteCalculator;
            RenderProperties = renderProperties;
            _gameStateProvider = gameStateProvider;
        }

        #region Game Component

        public override void Initialize()
        {
            _charRenderTarget = _renderTargetFactory.CreateRenderTarget();
            _sb = new SpriteBatch(Game.GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _characterTextures.Refresh(_renderProperties);

            if (_gameStateProvider.CurrentState == GameStates.None)
            {
                _outline = new Texture2D(GraphicsDevice, 1, 1);
                _outline.SetData(new[] { Color.Black });
            }

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (TopPixel == null)
            {
                TopPixel = FigureOutTopPixel(_characterSpriteCalculator, _renderProperties);
            }

            if (!Visible)
                return;

            if (_textureUpdateRequired)
            {
                _characterTextures.Refresh(_renderProperties);
                DrawToRenderTarget();

                _textureUpdateRequired = false;
            }

            if (_positionIsRelative)
                SetGridCoordinatePosition();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible || _sb.IsDisposed)
                return;

            //todo: check if this is the renderer for the main player
            //        if hidden, draw if: they are not active character and active character is admin

            _sb.Begin();
            DrawToSpriteBatch(_sb);
            
            if (_sb.IsDisposed)
                return;
            _sb.End();

            //todo: draw effect over character

            base.Draw(gameTime);
        }

        #endregion

        #region ICharacterRenderer

        public void SetAbsoluteScreenPosition(int xPosition, int yPosition)
        {
            SetScreenCoordinates(xPosition, yPosition);
            _positionIsRelative = false;
        }

        public void SetToCenterScreenPosition()
        {
            const int x = 314; // 618 / 2.0

            var skinRect = _characterTextures.Skin.SourceRectangle;
            var y = (298 - skinRect.Height)/2 - skinRect.Height/4;
            SetAbsoluteScreenPosition(x, y);
        }

        public void DrawToSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_charRenderTarget, Vector2.Zero, GetAlphaColor());
        }

        #endregion

        #region Texture Loading Helpers

        private static int FigureOutTopPixel(ICharacterSpriteCalculator spriteCalculator, ICharacterRenderProperties renderProperties)
        {
            var spriteForSkin = spriteCalculator.GetSkinTexture(renderProperties);
            var skinData = spriteForSkin.GetSourceTextureData<Color>();

            int i = 0;
            while (i < skinData.Length && skinData[i].A == 0) i++;

            var firstPixelHeight = i == skinData.Length - 1 ? 0 : i/spriteForSkin.SourceRectangle.Height;
            var genderOffset = renderProperties.Gender == 0 ? 12 : 13;

            return genderOffset + firstPixelHeight;
        }

        #endregion

        #region Drawing Helpers

        private void DrawToRenderTarget()
        {
            GraphicsDevice.SetRenderTarget(_charRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            _sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            var characterPropertyRenderers = _characterPropertyRendererBuilder
                .BuildList(_characterTextures, RenderProperties)
                .Where(x => x.CanRender);
            foreach (var renderer in characterPropertyRenderers)
                renderer.Render(_sb, DrawArea);

            if (_gameStateProvider.CurrentState == GameStates.None)
            {
                _sb.Draw(_outline, DrawArea.WithSize(DrawArea.Width, 1), Color.Black);
                _sb.Draw(_outline, DrawArea.WithPosition(new Vector2(DrawArea.X + DrawArea.Width, DrawArea.Y)).WithSize(1, DrawArea.Height), Color.Black);
                _sb.Draw(_outline, DrawArea.WithPosition(new Vector2(DrawArea.X, DrawArea.Y + DrawArea.Height)).WithSize(DrawArea.Width, 1), Color.Black);
                _sb.Draw(_outline, DrawArea.WithSize(1, DrawArea.Height), Color.Black);
            }

            _sb.End();
            GraphicsDevice.SetRenderTarget(null);
        }

        private Color GetAlphaColor()
        {
            return RenderProperties.IsHidden || RenderProperties.IsDead
                ? Color.FromNonPremultiplied(255, 255, 255, 128)
                : Color.White;
        }

        private void SetGridCoordinatePosition()
        {
            //todo: the constants here should be dynamically configurable to support window resizing
            var screenX = _renderOffsetCalculator.CalculateOffsetX(RenderProperties) + 312 - GetMainCharacterOffsetX();
            var screenY = _renderOffsetCalculator.CalculateOffsetY(RenderProperties) + 106 - GetMainCharacterOffsetY();

            SetScreenCoordinates(screenX, screenY);
        }

        private void SetScreenCoordinates(int xPosition, int yPosition)
        {
            // size of standing still skin texture
            DrawArea = new Rectangle(xPosition, yPosition, 18, 58);
            _textureUpdateRequired = true;
        }

        private int GetMainCharacterOffsetX()
        {
            return _renderOffsetCalculator.CalculateOffsetX(_characterProvider.MainCharacter.RenderProperties);
        }

        private int GetMainCharacterOffsetY()
        {
            return _renderOffsetCalculator.CalculateOffsetY(_characterProvider.MainCharacter.RenderProperties);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _outline?.Dispose();

                _sb.Dispose();
                _charRenderTarget.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
