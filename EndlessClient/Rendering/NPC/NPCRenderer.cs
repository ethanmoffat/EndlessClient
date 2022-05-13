using EndlessClient.Audio;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Spells;
using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Extensions;
using EOLib.Domain.NPC;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Optional;
using System;
using System.Linq;
using XNAControls;

namespace EndlessClient.Rendering.NPC
{
    public class NPCRenderer : DrawableGameComponent, INPCRenderer
    {
        // todo: load this from a config or find a better way
        // list: Reaper, Royal Guard, Elite Captain, Horse, Unicorn, Anundo Leader, Apozen
        private static readonly int[] _npcsThatAreNotCentered = new[] { 9, 57, 58, 66, 67, 120, 142 };

        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly INPCSpriteSheet _npcSpriteSheet;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly IHealthBarRendererFactory _healthBarRendererFactory;
        private readonly IChatBubbleFactory _chatBubbleFactory;
        private readonly INPCInteractionController _npcInteractionController;
        private readonly IMapInteractionController _mapInteractionController;
        private readonly IUserInputProvider _userInputProvider;
        private readonly ISpellSlotDataProvider _spellSlotDataProvider;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly Rectangle _baseTextureFrameRectangle;
        private readonly int _readonlyTopPixel, _readonlyBottomPixel;
        private readonly bool _hasStandingAnimation;
        private readonly IEffectRenderer _effectRenderer;
        private readonly IHealthBarRenderer _healthBarRenderer;

        private DateTime _lastStandingAnimation;
        private int _fadeAwayAlpha;
        private bool _isDying;

        private XNALabel _nameLabel;
        private IChatBubble _chatBubble;

        public int TopPixel => _readonlyTopPixel;

        public int BottomPixel => _readonlyBottomPixel;

        public int TopPixelWithOffset => _readonlyTopPixel + DrawArea.Y;

        public int BottomPixelWithOffset => _readonlyBottomPixel + DrawArea.Y;

        public Rectangle DrawArea { get; private set; }

        public Rectangle MapProjectedDrawArea { get; private set; }

        public EOLib.Domain.NPC.NPC NPC { get; set; }

        public bool IsDead { get; private set; }

        public Rectangle EffectTargetArea => DrawArea;

        public NPCRenderer(INativeGraphicsManager nativeGraphicsManager,
                           IEndlessGameProvider endlessGameProvider,
                           ICharacterRendererProvider characterRendererProvider,
                           IENFFileProvider enfFileProvider,
                           INPCSpriteSheet npcSpriteSheet,
                           IRenderOffsetCalculator renderOffsetCalculator,
                           IHealthBarRendererFactory healthBarRendererFactory,
                           IChatBubbleFactory chatBubbleFactory,
                           INPCInteractionController npcInteractionController,
                           IMapInteractionController mapInteractionController,
                           IUserInputProvider userInputProvider,
                           ISpellSlotDataProvider spellSlotDataProvider,
                           ISfxPlayer sfxPlayer,
                           EOLib.Domain.NPC.NPC initialNPC)
            : base((Game)endlessGameProvider.Game)
        {
            NPC = initialNPC;

            _characterRendererProvider = characterRendererProvider;
            _enfFileProvider = enfFileProvider;
            _npcSpriteSheet = npcSpriteSheet;
            _renderOffsetCalculator = renderOffsetCalculator;
            _healthBarRendererFactory = healthBarRendererFactory;
            _chatBubbleFactory = chatBubbleFactory;
            _npcInteractionController = npcInteractionController;
            _mapInteractionController = mapInteractionController;
            _userInputProvider = userInputProvider;
            _spellSlotDataProvider = spellSlotDataProvider;
            _sfxPlayer = sfxPlayer;

            _baseTextureFrameRectangle = GetStandingFrameRectangle();
            _readonlyTopPixel = GetTopPixel();
            _readonlyBottomPixel = GetBottomPixel();

            _hasStandingAnimation = GetHasStandingAnimation();
            _lastStandingAnimation = DateTime.Now;
            _fadeAwayAlpha = 255;

            _effectRenderer = new EffectRenderer(nativeGraphicsManager, _sfxPlayer, this);
            _healthBarRenderer = _healthBarRendererFactory.CreateHealthBarRenderer(this);
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
                Text = _enfFileProvider.ENFFile[NPC.ID].Name,
                DrawOrder = 30,
                KeepInClientWindowBounds = false,
            };
            _nameLabel.Initialize();

            if (!_nameLabel.Game.Components.Contains(_nameLabel))
                _nameLabel.Game.Components.Add(_nameLabel);

            _nameLabel.DrawPosition = GetNameLabelPosition();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible) return;

            UpdateDrawAreas();
            UpdateStandingFrameAnimation();
            UpdateDeadState();

            _nameLabel.Visible = DrawArea.Contains(_userInputProvider.CurrentMouseState.Position) && !_healthBarRenderer.Visible && !_isDying;
            _nameLabel.DrawPosition = GetNameLabelPosition();

            if (DrawArea.Contains(_userInputProvider.CurrentMouseState.Position) &&
                _userInputProvider.CurrentMouseState.LeftButton == ButtonState.Released &&
                _userInputProvider.PreviousMouseState.LeftButton == ButtonState.Pressed &&
                !_userInputProvider.ClickHandled)
            {
                if (_spellSlotDataProvider.SpellIsPrepared)
                    _mapInteractionController.LeftClick(NPC);
                else
                    _npcInteractionController.ShowNPCDialog(NPC);
            }

            _effectRenderer.Update();
            _healthBarRenderer.Update(gameTime);

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

            _healthBarRenderer.DrawToSpriteBatch(spriteBatch);
        }

        public void StartDying()
        {
            _isDying = true;
        }

        public void ShowDamageCounter(int damage, int percentHealth, bool isHeal)
        {
            var optionalDamage = damage.SomeWhen(d => d > 0);
            _healthBarRenderer.SetDamage(optionalDamage, percentHealth);
        }

        public void ShowChatBubble(string message, bool isGroupChat)
        {
            if (_chatBubble == null)
                _chatBubble = _chatBubbleFactory.CreateChatBubble(this);
            _chatBubble.SetMessage(message, isGroupChat: false);
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
            var frameData = new Color[frameTexture.Width * frameTexture.Height];
            frameTexture.GetData(frameData);

            int i = 0;
            while (i < frameData.Length && frameData[i].A == 0) i++;

            return i == frameData.Length - 1 ? 0 : i / frameTexture.Height;
        }

        private int GetBottomPixel()
        {
            var data = _enfFileProvider.ENFFile[NPC.ID];
            var frameTexture = _npcSpriteSheet.GetNPCTexture(data.Graphic, NPCFrame.Standing, EODirection.Down);
            var frameData = new Color[frameTexture.Width * frameTexture.Height];
            frameTexture.GetData(frameData);


            int i = frameData.Length - 1;
            while (i >= 0 && frameData[i].A != 0) i--;

            return i == frameData.Length - 1 ? frameTexture.Height : i / frameTexture.Height;
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

            _characterRendererProvider.MainCharacterRenderer
                .MatchSome(mainRenderer =>
                {
                    var mainOffsetX = _renderOffsetCalculator.CalculateOffsetX(mainRenderer.Character.RenderProperties);
                    var mainOffsetY = _renderOffsetCalculator.CalculateOffsetY(mainRenderer.Character.RenderProperties);

                    // Some NPCs have an off-center sprite that needs to be divided by 3 (normal sprites are centered properly)
                    // If e.g. Apozen is facing Down or Left it needs to be offset by 2/3 the sprite width instead of 1/3 the sprite width
                    var widthFactor = _npcsThatAreNotCentered.Contains(NPC.ID)
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
                        xCoord + widthFactor - 8,
                        BottomPixelWithOffset - (int)oneGridSize.Y,
                        (int)oneGridSize.X,
                        (int)oneGridSize.Y);
                });
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
            return new Vector2(MapProjectedDrawArea.X + (MapProjectedDrawArea.Width - _nameLabel.ActualWidth) / 2f,
                               TopPixelWithOffset - _nameLabel.ActualHeight - 8);

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
