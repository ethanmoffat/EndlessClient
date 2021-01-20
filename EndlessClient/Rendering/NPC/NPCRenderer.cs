using System;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Extensions;
using EOLib.Domain.NPC;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Rendering.NPC
{
    public class NPCRenderer : DrawableGameComponent, INPCRenderer
    {
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly INPCSpriteSheet _npcSpriteSheet;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly Rectangle _baseTextureFrameRectangle;
        private readonly int _readonlyTopPixel;
        private readonly bool _hasStandingAnimation;
        private readonly IEffectRenderer _effectRenderer;

        private DateTime _lastStandingAnimation;
        private int _fadeAwayAlpha;
        private bool _isDying;
        private MouseState _previousMouseState;
        private MouseState _currentMouseState;

        private XNALabel _nameLabel;

        public int TopPixel => _readonlyTopPixel;

        public Rectangle DrawArea { get; private set; }

        public Rectangle MapProjectedDrawArea { get; private set; }

        public INPC NPC { get; set; }

        public bool IsDead { get; private set; }

        public Rectangle EffectTargetArea => DrawArea;

        public NPCRenderer(INativeGraphicsManager nativeGraphicsManager,
                           IEndlessGameProvider endlessGameProvider,
                           ICharacterRendererProvider characterRendererProvider,
                           IENFFileProvider enfFileProvider,
                           INPCSpriteSheet npcSpriteSheet,
                           IRenderOffsetCalculator renderOffsetCalculator,
                           INPC initialNPC)
            : base((Game)endlessGameProvider.Game)
        {
            NPC = initialNPC;

            _characterRendererProvider = characterRendererProvider;
            _enfFileProvider = enfFileProvider;
            _npcSpriteSheet = npcSpriteSheet;
            _renderOffsetCalculator = renderOffsetCalculator;

            _baseTextureFrameRectangle = GetStandingFrameRectangle();
            _readonlyTopPixel = GetTopPixel();

            _hasStandingAnimation = GetHasStandingAnimation();
            _lastStandingAnimation = DateTime.Now;
            _fadeAwayAlpha = 255;

            _effectRenderer = new EffectRenderer(nativeGraphicsManager, this);
        }

        public override void Initialize()
        {
            UpdateDrawAreas();

            _nameLabel = new XNALabel(Constants.FontSize08pt5)
            {
                Visible = true,
                TextWidth = 89,
                TextAlign = LabelAlignment.MiddleCenter,
                ForeColor = Color.White,
                AutoSize = true,
                Text = _enfFileProvider.ENFFile[NPC.ID].Name
            };
            _nameLabel.Initialize();

            _nameLabel.DrawPosition = GetNameLabelPosition();
            _previousMouseState = _currentMouseState = Mouse.GetState();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible) return;

            _currentMouseState = Mouse.GetState();

            UpdateDrawAreas();
            UpdateStandingFrameAnimation();
            UpdateDeadState();

            _nameLabel.Visible = DrawArea.Contains(_currentMouseState.Position);
            _nameLabel.DrawPosition = GetNameLabelPosition();
            _nameLabel.Update(gameTime);

            _effectRenderer.Update();

            _previousMouseState = _currentMouseState;

            base.Update(gameTime);
        }

        public void DrawToSpriteBatch(SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            var data = _enfFileProvider.ENFFile[NPC.ID];

            var color = Color.FromNonPremultiplied(255, 255, 255, _fadeAwayAlpha);
            var effects = NPC.IsFacing(EODirection.Left, EODirection.Down) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            _effectRenderer.DrawBehindTarget(spriteBatch);
            spriteBatch.Draw(_npcSpriteSheet.GetNPCTexture(data.Graphic, NPC.Frame, NPC.Direction),
                             DrawArea, null, color, 0f, Vector2.Zero, effects, 1f);
            _effectRenderer.DrawInFrontOfTarget(spriteBatch);

            _nameLabel.Draw(new GameTime());
        }

        public void StartDying()
        {
            _isDying = true;
        }

        #region Effects

        public bool EffectIsPlaying()
        {
            return _effectRenderer.State == EffectState.Playing;
        }

        public void ShowWaterSplashies() { }

        public void ShowWarpArrive() { }

        public void ShowWarpLeave() { }

        public void ShowPotionAnimation(int potionId) { }

        public void ShowSpellAnimation(int spellId)
        {
            _effectRenderer.PlayEffect(EffectType.Spell, spellId);
        }

        #endregion

        private Rectangle GetStandingFrameRectangle()
        {
            var data = _enfFileProvider.ENFFile[NPC.ID];
            var baseFrame = _npcSpriteSheet.GetNPCTexture(data.Graphic, NPCFrame.Standing, EODirection.Down);
            return new Rectangle(0, 0, baseFrame.Width, baseFrame.Height);
        }

        private int GetTopPixel()
        {
            var data = _enfFileProvider.ENFFile[NPC.ID];
            var frameTexture = _npcSpriteSheet.GetNPCTexture(data.Graphic, NPCFrame.Standing, EODirection.Down);
            var frameTextureData = new Color[frameTexture.Width * frameTexture.Height];
            frameTexture.GetData(frameTextureData);

            if (frameTextureData.All(x => x.A == 0))
                return 0;

            var firstVisiblePixelIndex = frameTextureData.Select((color, index) => new { color, index })
                                                            .Where(x => x.color.A != 0)
                                                            .Select(x => x.index)
                                                            .First();
            return firstVisiblePixelIndex/frameTexture.Height;
        }

        private bool GetHasStandingAnimation()
        {
            var data = _enfFileProvider.ENFFile[NPC.ID];

            var frameTexture = _npcSpriteSheet.GetNPCTexture(data.Graphic, NPCFrame.StandingFrame1, NPC.Direction);
            var textureData = new Color[frameTexture.Width * frameTexture.Height];
            frameTexture.GetData(textureData);

            return textureData.Any(color => ((color.R > 0 && color.R != 8) || (color.G > 0 && color.G != 8) || (color.B > 0 && color.B != 8)) && color.A > 0);
        }

        private void UpdateDrawAreas()
        {
            var offsetX = _renderOffsetCalculator.CalculateOffsetX(NPC);
            var offsetY = _renderOffsetCalculator.CalculateOffsetY(NPC);

            var mainRenderer = _characterRendererProvider.MainCharacterRenderer;
            var mainOffsetX = _renderOffsetCalculator.CalculateOffsetX(mainRenderer.Character.RenderProperties);
            var mainOffsetY = _renderOffsetCalculator.CalculateOffsetY(mainRenderer.Character.RenderProperties);

            // Apozen is a wider sprite that needs to be divided by 3 (normal sprites are centered properly)
            // If Apozen is facing Down or Left it needs to be offset by 2/3 the sprite width instead of 1/3 the sprite width
            // I'm guessing the presence of the RGB (8,8,8) pixels in StandingFrame1 is used as a mask to determine
            //    the proper width for the offset but this garbage is fine for now
            var widthFactor = _baseTextureFrameRectangle.Width > 120
                ? NPC.IsFacing(EODirection.Down, EODirection.Left)
                    ? (_baseTextureFrameRectangle.Width * 2) / 3
                    : _baseTextureFrameRectangle.Width / 3
                : _baseTextureFrameRectangle.Width / 2;

            // y coordinate Formula courtesy of Apollo
            var xCoord = offsetX + 320 - mainOffsetX - widthFactor;
            var yCoord = (Math.Min(41, _baseTextureFrameRectangle.Width - 23) / 4) + offsetY + 168 - mainOffsetY - _baseTextureFrameRectangle.Height;
            DrawArea = _baseTextureFrameRectangle.WithPosition(new Vector2(xCoord, yCoord));

            var oneGridSize = new Vector2(mainRenderer.DrawArea.Width,
                                          mainRenderer.DrawArea.Height);
            MapProjectedDrawArea = new Rectangle(
                DrawArea.X + (int)(Math.Abs(oneGridSize.X - DrawArea.Width) / 2),
                DrawArea.Bottom - (int)oneGridSize.Y,
                (int)oneGridSize.X,
                (int)oneGridSize.Y);
        }

        private void UpdateStandingFrameAnimation()
        {
            var now = DateTime.Now;

            if (!_hasStandingAnimation
                || !NPC.IsActing(NPCActionState.Standing)
                || (now - _lastStandingAnimation).TotalMilliseconds < 250)
                return;

            _lastStandingAnimation = now;
            NPC = NPC.WithFrame(NPC.Frame == NPCFrame.Standing ? NPCFrame.StandingFrame1 : NPCFrame.Standing);
        }

        private void UpdateDeadState()
        {
            if (!_isDying) return;

            if (_fadeAwayAlpha >= 3)
                _fadeAwayAlpha -= 3;
            IsDead = _fadeAwayAlpha <= 0 && !EffectIsPlaying();
        }

        private Vector2 GetNameLabelPosition()
        {
            return new Vector2(DrawArea.X + (DrawArea.Width - _nameLabel.ActualWidth) / 2,
                               DrawArea.Y + TopPixel - _nameLabel.ActualHeight - 4);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nameLabel.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
