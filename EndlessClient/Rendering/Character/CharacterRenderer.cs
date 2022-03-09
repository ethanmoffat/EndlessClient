using System;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Sprites;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Rendering.Character
{
    public class CharacterRenderer : DrawableGameComponent, ICharacterRenderer
    {
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly IHealthBarRendererFactory _healthBarRendererFactory;
        private readonly IChatBubbleFactory _chatBubbleFactory;
        private readonly ICharacterProvider _characterProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly ICharacterPropertyRendererBuilder _characterPropertyRendererBuilder;
        private readonly ICharacterTextures _characterTextures;
        private readonly ICharacterSpriteCalculator _characterSpriteCalculator;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IEffectRenderer _effectRenderer;

        private ICharacter _character;
        private bool _textureUpdateRequired, _positionIsRelative = true;
        private MouseState _previousMouseState;
        private MouseState _currentMouseState;

        private SpriteBatch _sb;
        private RenderTarget2D _charRenderTarget;
        private Texture2D _outline;

        private BlinkingLabel _nameLabel;
        private string _shoutName = string.Empty;
        private DateTime? _spellCastTime;

        private IHealthBarRenderer _healthBarRenderer;
        private IChatBubble _chatBubble;

        public ICharacter Character
        {
            get { return _character; }
            set
            {
                if (_character == value) return;
                _textureUpdateRequired = _character.RenderProperties.GetHashCode() != value.RenderProperties.GetHashCode();
                _character = value;
            }
        }

        public bool Transparent { get; set; }

        public Rectangle DrawArea { get; private set; }

        public Rectangle MapProjectedDrawArea => DrawArea;

        private int? _topPixel;
        public int TopPixel => _topPixel.HasValue ? _topPixel.Value : 0;

        public int TopPixelWithOffset => TopPixel + DrawArea.Y;

        public Rectangle EffectTargetArea
            => DrawArea.WithPosition(new Vector2(DrawArea.X, DrawArea.Y - 8));

        public CharacterRenderer(INativeGraphicsManager nativeGraphicsmanager,
                                 Game game,
                                 IRenderTargetFactory renderTargetFactory,
                                 IHealthBarRendererFactory healthBarRendererFactory,
                                 IChatBubbleFactory chatBubbleFactory,
                                 ICharacterProvider characterProvider,
                                 IRenderOffsetCalculator renderOffsetCalculator,
                                 ICharacterPropertyRendererBuilder characterPropertyRendererBuilder,
                                 ICharacterTextures characterTextures,
                                 ICharacterSpriteCalculator characterSpriteCalculator,
                                 ICharacter character,
                                 IGameStateProvider gameStateProvider,
                                 ICurrentMapProvider currentMapProvider)
            : base(game)
        {
            _renderTargetFactory = renderTargetFactory;
            _healthBarRendererFactory = healthBarRendererFactory;
            _chatBubbleFactory = chatBubbleFactory;
            _characterProvider = characterProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
            _characterPropertyRendererBuilder = characterPropertyRendererBuilder;
            _characterTextures = characterTextures;
            _characterSpriteCalculator = characterSpriteCalculator;
            _character = character;
            _gameStateProvider = gameStateProvider;
            _currentMapProvider = currentMapProvider;
            _effectRenderer = new EffectRenderer(nativeGraphicsmanager, this);
        }

        #region Game Component

        public override void Initialize()
        {
            _charRenderTarget = _renderTargetFactory.CreateRenderTarget();
            _sb = new SpriteBatch(Game.GraphicsDevice);

            _nameLabel = new BlinkingLabel(Constants.FontSize08pt5)
            {
                Visible = _gameStateProvider.CurrentState == GameStates.PlayingTheGame,
                TextWidth = 89,
                TextAlign = LabelAlignment.MiddleCenter,
                ForeColor = Color.White,
                AutoSize = true,
                Text = _character?.Name ?? string.Empty,
                DrawOrder = 30,
                KeepInClientWindowBounds = false,
            };
            _nameLabel.Initialize();

            if (!_nameLabel.Game.Components.Contains(_nameLabel))
                _nameLabel.Game.Components.Add(_nameLabel);

            _nameLabel.DrawPosition = GetNameLabelPosition();
            _previousMouseState = _currentMouseState = Mouse.GetState();

            _healthBarRenderer = _healthBarRendererFactory.CreateHealthBarRenderer(this);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _characterTextures.Refresh(_character.RenderProperties);

            _outline = new Texture2D(GraphicsDevice, 1, 1);
            _outline.SetData(new[] { Color.White });

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            _topPixel = _topPixel ?? FigureOutTopPixel(_characterSpriteCalculator, _character.RenderProperties);

            // Effects can be rendered when character is not visible (leaving map)
            _effectRenderer.Update();

            if (!Visible)
                return;

            _currentMouseState = Mouse.GetState();

            if (_textureUpdateRequired)
            {
                _characterTextures.Refresh(_character.RenderProperties);
                DrawToRenderTarget();

                _textureUpdateRequired = false;
            }

            if (_positionIsRelative)
                SetGridCoordinatePosition();

            if (_gameStateProvider.CurrentState == GameStates.PlayingTheGame)
                UpdateNameLabel(gameTime);

            _healthBarRenderer.Update(gameTime);

            _previousMouseState = _currentMouseState;

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
            _effectRenderer.DrawBehindTarget(spriteBatch);
            if (Visible)
                spriteBatch.Draw(_charRenderTarget, new Vector2(0, GetSteppingStoneOffset(Character.RenderProperties)), GetAlphaColor());
            _effectRenderer.DrawInFrontOfTarget(spriteBatch);

            _healthBarRenderer.DrawToSpriteBatch(spriteBatch);
        }

        #endregion

        #region Texture Loading Helpers

        private static int FigureOutTopPixel(ICharacterSpriteCalculator spriteCalculator, ICharacterRenderProperties renderProperties)
        {
            var spriteForSkin = spriteCalculator.GetSkinTexture(renderProperties);
            var skinData = spriteForSkin.GetSourceTextureData<Color>();

            int i = 0;
            while (i < skinData.Length && skinData[i].A == 0) i++;

            return i == skinData.Length - 1 ? 0 : i/spriteForSkin.SourceRectangle.Height;
        }

        #endregion

        #region Update/Drawing Helpers

        private void DrawToRenderTarget()
        {
            GraphicsDevice.SetRenderTarget(_charRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);
            _sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            var characterPropertyRenderers = _characterPropertyRendererBuilder
                .BuildList(_characterTextures, _character.RenderProperties)
                .Where(x => x.CanRender);
            foreach (var renderer in characterPropertyRenderers)
                renderer.Render(_sb, DrawArea);

            if (_gameStateProvider.CurrentState == GameStates.None)
            {
                _sb.Draw(_outline, DrawArea.WithSize(DrawArea.Width, 1), Color.Black);
                _sb.Draw(_outline, DrawArea.WithPosition(new Vector2(DrawArea.X + DrawArea.Width, DrawArea.Y)).WithSize(1, DrawArea.Height), Color.Black);
                _sb.Draw(_outline, DrawArea.WithPosition(new Vector2(DrawArea.X, DrawArea.Y + DrawArea.Height)).WithSize(DrawArea.Width, 1), Color.Black);
                _sb.Draw(_outline, DrawArea.WithSize(1, DrawArea.Height), Color.Black);

                _sb.Draw(_outline, DrawArea, Color.FromNonPremultiplied(255, 0, 0, 64));
                _sb.Draw(_outline, MapProjectedDrawArea, Color.FromNonPremultiplied(0, 255, 0, 64));
            }

            _sb.End();
            GraphicsDevice.SetRenderTarget(null);
        }

        private Color GetAlphaColor()
        {
            // don't render the transparent character layer if hidden/dead, otherwise the additive blending 
            //      will render it with full alpha
            if (_character.RenderProperties.IsHidden || _character.RenderProperties.IsDead)
                return Transparent ? Color.Transparent : Color.FromNonPremultiplied(255, 255, 255, 128);

            return Transparent
                ? Color.FromNonPremultiplied(255, 255, 255, 128)
                : Color.White;
        }

        private void SetGridCoordinatePosition()
        {
            //todo: the constants here should be dynamically configurable to support window resizing
            var screenX = _renderOffsetCalculator.CalculateOffsetX(_character.RenderProperties) + 312 - GetMainCharacterOffsetX();
            var screenY = _renderOffsetCalculator.CalculateOffsetY(_character.RenderProperties) + 106 - GetMainCharacterOffsetY();

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

        private void UpdateNameLabel(GameTime gameTime)
        {
            if (_healthBarRenderer.Visible)
            {
                _nameLabel.Visible = false;
            }
            else if (DrawArea.Contains(_currentMouseState.Position))
            {
                _nameLabel.Visible = true;
                _nameLabel.BlinkRate = null;
                _nameLabel.Text = _character.Name;
            }
            else if (_shoutName != string.Empty && _nameLabel.Text != _shoutName)
            {
                _nameLabel.Visible = true;
                _nameLabel.BlinkRate = 250;
                _nameLabel.Text = _shoutName;
            }
            else if (_shoutName == string.Empty)
            {
                _nameLabel.Visible = false;
            }

            if (_spellCastTime.HasValue && (DateTime.Now - _spellCastTime.Value).TotalMilliseconds >= 600)
            {
                _nameLabel.Visible = false;
                _nameLabel.Text = _character.Name;
                _nameLabel.ForeColor = Color.White;
                _nameLabel.BlinkRate = null;
                _shoutName = string.Empty;
                _spellCastTime = null;
            }

            _nameLabel.DrawPosition = GetNameLabelPosition();
        }

        private Vector2 GetNameLabelPosition()
        {
            return new Vector2(DrawArea.X - Math.Abs(DrawArea.Width - _nameLabel.ActualWidth) / 2,
                               TopPixelWithOffset - 8 - _nameLabel.ActualHeight);
        }

        private bool GetIsSteppingStone(ICharacterRenderProperties renderProps)
        {
            if (_gameStateProvider.CurrentState != GameStates.PlayingTheGame)
                return false;

            return _currentMapProvider.CurrentMap.Tiles[renderProps.MapY, renderProps.MapX] == TileSpec.Jump
                || (renderProps.IsActing(CharacterActionState.Walking) && _currentMapProvider.CurrentMap.Tiles[renderProps.GetDestinationY(), renderProps.GetDestinationX()] == TileSpec.Jump);
        }

        private int GetSteppingStoneOffset(ICharacterRenderProperties renderProps)
        {
            var isSteppingStone = GetIsSteppingStone(renderProps);

            if (isSteppingStone && renderProps.IsActing(CharacterActionState.Walking))
            {
                switch(renderProps.ActualWalkFrame)
                {
                    case 1: return -8;
                    case 2: return -16;
                    case 3: return -16;
                    case 4: return -8;
                }
            }

            return 0;
        }

        #endregion

        #region Effects

        public bool EffectIsPlaying()
        {
            return _effectRenderer.State == EffectState.Playing;
        }

        public void ShowWaterSplashies()
        {
            if (_effectRenderer.EffectType == EffectType.WaterSplashies &&
                _effectRenderer.State == EffectState.Playing)
                _effectRenderer.Restart();

            _effectRenderer.PlayEffect(EffectType.WaterSplashies, 0);
        }

        public void ShowWarpArrive()
        {
            _effectRenderer.PlayEffect(EffectType.WarpDestination, 0);
        }

        public void ShowWarpLeave()
        {
            _effectRenderer.PlayEffect(EffectType.WarpOriginal, 0);
        }

        public void ShowPotionAnimation(int potionId)
        {
            _effectRenderer.PlayEffect(EffectType.Potion, potionId);
        }

        public void ShowSpellAnimation(int spellId)
        {
            _effectRenderer.PlayEffect(EffectType.Spell, spellId);
        }

        #endregion

        // Called when the spell cast begins
        public void ShoutSpellPrep(string spellName)
        {
            _shoutName = spellName;
        }

        // Called when the spell prep time ends and the player actually casts the spell
        public void ShoutSpellCast()
        {
            _nameLabel.BlinkRate = null;
            _nameLabel.ForeColor = Color.FromNonPremultiplied(0xf5, 0xc8, 0x9c, 0xff); // todo: make constant for this
            _spellCastTime = DateTime.Now;
        }

        // Called when the shout (spell prep time) should be cancelled without casting
        public void StopShoutingSpell()
        {
            _shoutName = string.Empty;
        }

        public void ShowDamageCounter(int damage, int percentHealth, bool isHeal)
        {
            if (isHeal)
                _healthBarRenderer.SetHealth(damage, percentHealth);
            else
                _healthBarRenderer.SetDamage(damage == 0 ? Optional<int>.Empty : new Optional<int>(damage), percentHealth);
        }

        public void ShowChatBubble(string message, bool isGroupChat)
        {
            if (_chatBubble == null)
                _chatBubble = _chatBubbleFactory.CreateChatBubble(this);
            _chatBubble.SetMessage(message, isGroupChat);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _outline?.Dispose();
                _nameLabel.Dispose();

                _sb.Dispose();
                _charRenderTarget.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
