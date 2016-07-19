// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Old;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
    public class CharacterRenderer : DrawableGameComponent, ICharacterRenderer
    {
        private readonly INativeGraphicsManager _graphicsManager;
        private readonly IItemFileProvider _itemFileProvider;

        private ICharacterSpriteCalculator _spriteCalculator;
        private ICharacterRenderProperties _characterRenderPropertiesPrivate;
        private ICharacterTextures _textures;
        private bool _textureUpdateRequired;

        private SpriteBatch _sb;
        private RenderTarget2D _charRenderTarget;

        public ICharacterRenderProperties RenderProperties
        {
            get { return _characterRenderPropertiesPrivate; }
            set
            {
                if (_characterRenderPropertiesPrivate == value) return;
                _characterRenderPropertiesPrivate = value;
                _textureUpdateRequired = true;
            }
        }

        public Rectangle DrawArea { get; private set; }

        public int TopPixel { get; private set; }

        public CharacterRenderer(Game game,
                                 INativeGraphicsManager graphicsManager,
                                 IItemFileProvider itemFileProvider,
                                 ICharacterRenderProperties renderProperties)
            : base(game)
        {
            _graphicsManager = graphicsManager;
            _itemFileProvider = itemFileProvider;
            RenderProperties = renderProperties;
        }

        #region Game Component

        public override void Initialize()
        {
            _charRenderTarget = new RenderTarget2D(Game.GraphicsDevice,
                Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
            _sb = new SpriteBatch(Game.GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            ReloadTextures();
            FigureOutTopPixel();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Game.IsActive || !Visible)
                return;

            if (_textureUpdateRequired)
            {
                ReloadTextures();
                DrawToRenderTarget();

                _textureUpdateRequired = false;
            }

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
            var skinRect = _textures.Skin.SourceRectangle;
            DrawArea = new Rectangle(xPosition, yPosition, skinRect.Width, skinRect.Height);
            _textureUpdateRequired = true;
        }

        public void SetGridCoordinatePosition(int xCoord, int yCoord, int mainCharacterOffsetX, int mainCharacterOffsetY)
        {
            //todo: constructor inject a provider for the main character so the offsets aren't parameters

            //todo: the constants here should be dynamically configurable to support window resizing
            var displayX = CalculateDisplayCoordinateX(xCoord, yCoord) + 304 - mainCharacterOffsetX;
            var displayY = CalculateDisplayCoordinateY(yCoord, yCoord) + 91 - mainCharacterOffsetY;

            SetAbsoluteScreenPosition(displayX, displayY);
        }

        public void DrawToSpriteBatch(SpriteBatch spriteBatch)
        {
            _sb.Draw(_charRenderTarget, Vector2.Zero, GetAlphaColor());
        }

        #endregion

        #region Texture Loading Helpers

        private void FigureOutTopPixel()
        {
            var spriteForSkin = _spriteCalculator.GetSkinTexture(isBow: false);
            var skinData = spriteForSkin.GetSourceTextureData<Color>();

            int i = 0;
            while (i < skinData.Length && skinData[i].A == 0) i++;

            var firstPixelHeight = i == skinData.Length - 1 ? 0 : i/spriteForSkin.SourceRectangle.Height;
            var genderOffset = RenderProperties.Gender == 0 ? 12 : 13;

            TopPixel = genderOffset + firstPixelHeight;
        }

        private void ReloadTextures()
        {
            _spriteCalculator = new CharacterSpriteCalculator(_graphicsManager, _characterRenderPropertiesPrivate);
            _textures = new CharacterTextures(_spriteCalculator);

            _textures.RefreshTextures(IsBowEquipped(), IsShieldOnBack());
        }

        #endregion

        #region Drawing Helpers

        private void DrawToRenderTarget()
        {
            GraphicsDevice.SetRenderTarget(_charRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            var characterPropertyRenderers = GetOrderedRenderers();
            foreach (var renderer in characterPropertyRenderers)
                renderer.Render(DrawArea);

            _sb.End();
            GraphicsDevice.SetRenderTarget(null);
        }

        private IEnumerable<ICharacterPropertyRenderer> GetOrderedRenderers()
        {
            var propertyListBuilder = new CharacterPropertyRendererBuilder(_sb, RenderProperties, _textures, _itemFileProvider);
            return propertyListBuilder.BuildList(IsShieldOnBack());
        }

        private Color GetAlphaColor()
        {
            return RenderProperties.IsHidden || RenderProperties.IsDead
                ? Color.FromNonPremultiplied(255, 255, 255, 128)
                : Color.White;
        }

        private int CalculateDisplayCoordinateX(int xCoord, int yCoord)
        {
            var multiplier = RenderProperties.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;
            var walkAdjust = RenderProperties.IsActing(CharacterActionState.Walking) ? 8 * RenderProperties.WalkFrame : 0;
            return xCoord * 32 - yCoord * 32 + walkAdjust * multiplier;
        }

        private int CalculateDisplayCoordinateY(int xCoord, int yCoord)
        {
            var multiplier = RenderProperties.IsFacing(EODirection.Left, EODirection.Up) ? -1 : 1;
            var walkAdjust = RenderProperties.IsActing(CharacterActionState.Walking) ? 4 * RenderProperties.WalkFrame : 0;
            return xCoord * 16 + yCoord * 16 + walkAdjust * multiplier;
        }

        #endregion

        #region Conditional Rendering Helpers

        private bool IsBowEquipped()
        {
            if (ItemFile == null || ItemFile.Data == null)
                return false;

            var itemData = ItemFile.Data;
            var weaponInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Weapon &&
                                                           x.DollGraphic == RenderProperties.WeaponGraphic);

            return weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged;
        }

        private bool IsShieldOnBack()
        {
            if (ItemFile == null || ItemFile.Data == null)
                return false;

            var itemData = ItemFile.Data;
            var shieldInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Shield &&
                                                           x.DollGraphic == RenderProperties.ShieldGraphic);

            return shieldInfo != null &&
                   (shieldInfo.Name == "Bag" ||
                    shieldInfo.SubType == ItemSubType.Arrows ||
                    shieldInfo.SubType == ItemSubType.Wings);
        }

        private IDataFile<ItemRecord> ItemFile { get { return _itemFileProvider.ItemFile; } }

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
