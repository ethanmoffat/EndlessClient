// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
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

        private ICharacterRenderProperties _renderProperties;
        private bool _textureUpdateRequired, _positionIsRelative = true;

        private SpriteBatch _sb;
        private RenderTarget2D _charRenderTarget;

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
                                 ICharacterRenderProperties renderProperties)
            : base(game)
        {
            _renderTargetFactory = renderTargetFactory;
            _characterProvider = characterProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
            _characterPropertyRendererBuilder = characterPropertyRendererBuilder;
            _characterTextures = characterTextures;
            _characterSpriteCalculator = characterSpriteCalculator;
            RenderProperties = renderProperties;
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
            var skinRect = _characterTextures.Skin.SourceRectangle;
            var x = (618 - skinRect.Width)/2 + 4;
            var y = (298 - skinRect.Height)/2 - 29;
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
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            var characterPropertyRenderers = _characterPropertyRendererBuilder
                .BuildList(_characterTextures, RenderProperties)
                .Where(x => x.CanRender);
            foreach (var renderer in characterPropertyRenderers)
                renderer.Render(_sb, DrawArea);

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
            var screenX = _renderOffsetCalculator.CalculateOffsetX(RenderProperties) + 304 - GetMainCharacterOffsetX();
            var screenY = _renderOffsetCalculator.CalculateOffsetY(RenderProperties) + 91 - GetMainCharacterOffsetY();

            SetScreenCoordinates(screenX, screenY);
        }

        private void SetScreenCoordinates(int xPosition, int yPosition)
        {
            var skinRect = _characterTextures.Skin.SourceRectangle;
            DrawArea = new Rectangle(xPosition, yPosition, skinRect.Width, skinRect.Height);
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
                _sb.Dispose();
                _charRenderTarget.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
