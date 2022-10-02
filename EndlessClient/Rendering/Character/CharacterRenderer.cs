using EndlessClient.Audio;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Sprites;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Optional;
using System;
using System.Diagnostics;
using System.Linq;
using XNAControls;

namespace EndlessClient.Rendering.Character
{
    public class CharacterRenderer : DrawableGameComponent, ICharacterRenderer
    {
        private readonly object _rt_locker_ = new object();

        private readonly IMapInteractionController _mapInteractionController;
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
        private readonly IUserInputProvider _userInputProvider;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IClientWindowSizeRepository _clientWindowSizeRepository;
        private readonly IFixedTimeStepRepository _fixedTimeStepRepository;
        private readonly IEffectRenderer _effectRenderer;

        private EOLib.Domain.Character.Character _character;
        private bool _textureUpdateRequired, _positionIsRelative = true;

        private SpriteBatch _sb;
        private RenderTarget2D _charRenderTarget;
        private Texture2D _outline;

        private BlinkingLabel _nameLabel;
        private string _shoutName = string.Empty;
        private DateTime? _spellCastTime;
        private bool _showName = true;

        private IHealthBarRenderer _healthBarRenderer;
        private Lazy<IChatBubble> _chatBubble;

        private bool _lastIsDead;

        public EOLib.Domain.Character.Character Character
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

        public bool MouseOver => DrawArea.Contains(_userInputProvider.CurrentMouseState.Position);

        public bool MouseOverPreviously => DrawArea.Contains(_userInputProvider.PreviousMouseState.Position);

        public Rectangle EffectTargetArea
            => DrawArea.WithPosition(new Vector2(DrawArea.X, DrawArea.Y - 8));

        public CharacterRenderer(INativeGraphicsManager nativeGraphicsmanager,
                                 Game game,
                                 IMapInteractionController mapInteractionController,
                                 IRenderTargetFactory renderTargetFactory,
                                 IHealthBarRendererFactory healthBarRendererFactory,
                                 IChatBubbleFactory chatBubbleFactory,
                                 ICharacterProvider characterProvider,
                                 IRenderOffsetCalculator renderOffsetCalculator,
                                 ICharacterPropertyRendererBuilder characterPropertyRendererBuilder,
                                 ICharacterTextures characterTextures,
                                 ICharacterSpriteCalculator characterSpriteCalculator,
                                 EOLib.Domain.Character.Character character,
                                 IGameStateProvider gameStateProvider,
                                 ICurrentMapProvider currentMapProvider,
                                 IUserInputProvider userInputProvider,
                                 ISfxPlayer sfxPlayer,
                                 IClientWindowSizeRepository clientWindowSizeRepository,
                                 IFixedTimeStepRepository fixedTimeStepRepository)
            : base(game)
        {
            _mapInteractionController = mapInteractionController;
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
            _userInputProvider = userInputProvider;
            _sfxPlayer = sfxPlayer;
            _clientWindowSizeRepository = clientWindowSizeRepository;
            _fixedTimeStepRepository = fixedTimeStepRepository;

            _effectRenderer = new EffectRenderer(nativeGraphicsmanager, _sfxPlayer, this);
            _chatBubble = new Lazy<IChatBubble>(() => _chatBubbleFactory.CreateChatBubble(this));

            _clientWindowSizeRepository.GameWindowSizeChanged += (_, _) =>
            {
                lock (_rt_locker_)
                {
                    _charRenderTarget.Dispose();
                    _charRenderTarget = _renderTargetFactory.CreateRenderTarget();
                }
            };

            if (_character == _characterProvider.MainCharacter)
                _clientWindowSizeRepository.GameWindowSizeChanged += (_, _) => SetToCenterScreenPosition();
        }

        #region Game Component

        public override void Initialize()
        {
            lock(_rt_locker_)
                _charRenderTarget = _renderTargetFactory.CreateRenderTarget();

            _sb = new SpriteBatch(Game.GraphicsDevice);

            if (_gameStateProvider.CurrentState == GameStates.PlayingTheGame)
            {
                _nameLabel = new BlinkingLabel(Constants.FontSize08pt5)
                {
                    Visible = true,
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

                _healthBarRenderer = _healthBarRendererFactory.CreateHealthBarRenderer(this);
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _textureUpdateRequired = true;
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

            if (_fixedTimeStepRepository.IsUpdateFrame)
            {
                if (_positionIsRelative)
                    SetGridCoordinatePosition();

                if (_textureUpdateRequired)
                {
                    _characterTextures.Refresh(_character.RenderProperties);
                    DrawToRenderTarget();

                    _textureUpdateRequired = false;
                }
            }

            if (_gameStateProvider.CurrentState == GameStates.PlayingTheGame)
            {
                UpdateNameLabel();

                if (MouseOver)
                {
                    if (_userInputProvider.CurrentMouseState.RightButton == ButtonState.Released &&
                        _userInputProvider.PreviousMouseState.RightButton == ButtonState.Pressed)
                    {
                        _mapInteractionController.RightClick(Character);
                    }
                    else if (_userInputProvider.CurrentMouseState.LeftButton == ButtonState.Released &&
                             _userInputProvider.PreviousMouseState.LeftButton == ButtonState.Pressed &&
                             !_userInputProvider.ClickHandled)
                    {
                        _mapInteractionController.LeftClick(Character);
                    }
                }

                _healthBarRenderer?.Update(gameTime);

                CheckForDead();
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible || _sb.IsDisposed)
                return;

            if (!Character.RenderProperties.IsHidden || _characterProvider.MainCharacter.AdminLevel > 0)
            {
                _sb.Begin();
                DrawToSpriteBatch(_sb);

                if (_sb.IsDisposed)
                    return;
                _sb.End();
            }

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

            var xCoord = (_clientWindowSizeRepository.Width - skinRect.Width) / 2;
            var yCoord = (_clientWindowSizeRepository.Height - skinRect.Height) / 2 - (_clientWindowSizeRepository.Resizable ? 11 : 0);

            SetAbsoluteScreenPosition(xCoord, yCoord);
        }

        public void DrawToSpriteBatch(SpriteBatch spriteBatch)
        {
            _effectRenderer.DrawBehindTarget(spriteBatch);

            if (Visible)
            {
                lock (_rt_locker_)
                {
                    spriteBatch.Draw(_charRenderTarget, new Vector2(0, GetSteppingStoneOffset(Character.RenderProperties)), GetAlphaColor());
                }
            }

            _effectRenderer.DrawInFrontOfTarget(spriteBatch);

            if (_gameStateProvider.CurrentState == GameStates.PlayingTheGame)
                _healthBarRenderer?.DrawToSpriteBatch(spriteBatch);
        }

        public void ShowName() => _showName = true;

        public void HideName() => _showName = false;

        #endregion

        #region Texture Loading Helpers

        private static int FigureOutTopPixel(ICharacterSpriteCalculator spriteCalculator, CharacterRenderProperties renderProperties)
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
            lock (_rt_locker_)
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
            var centerX = _clientWindowSizeRepository.Width / 2;
            var centerY = _clientWindowSizeRepository.Height * 45 / 100;

            var screenX = _renderOffsetCalculator.CalculateOffsetX(_character.RenderProperties) + centerX - GetMainCharacterOffsetX() - (_clientWindowSizeRepository.Resizable ? 8 : 0);
            var screenY = _renderOffsetCalculator.CalculateOffsetY(_character.RenderProperties) + centerY - GetMainCharacterOffsetY();

            SetScreenCoordinates(screenX, screenY);
        }

        private void SetScreenCoordinates(int xPosition, int yPosition)
        {
            if (DrawArea.X != xPosition || DrawArea.Y != yPosition)
            {
                // size of standing still skin texture
                DrawArea = new Rectangle(xPosition, yPosition, 18, 58);
                _textureUpdateRequired = true;
            }
        }

        private int GetMainCharacterOffsetX()
        {
            return _renderOffsetCalculator.CalculateOffsetX(_characterProvider.MainCharacter.RenderProperties);
        }

        private int GetMainCharacterOffsetY()
        {
            return _renderOffsetCalculator.CalculateOffsetY(_characterProvider.MainCharacter.RenderProperties);
        }

        private void UpdateNameLabel()
        {
            if (_gameStateProvider.CurrentState != GameStates.PlayingTheGame ||
                _healthBarRenderer == null ||
                _nameLabel == null)
                return;

            if (_healthBarRenderer.Visible)
            {
                _nameLabel.Visible = false;
            }
            else if (DrawArea.Contains(_userInputProvider.CurrentMouseState.Position) && _showName)
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
                StopShout();
            }

            _nameLabel.DrawPosition = GetNameLabelPosition();
        }

        private Vector2 GetNameLabelPosition()
        {
            return new Vector2(DrawArea.X - Math.Abs(DrawArea.Width - _nameLabel.ActualWidth) / 2,
                               TopPixelWithOffset - 8 - _nameLabel.ActualHeight);
        }

        private bool GetIsSteppingStone(CharacterRenderProperties renderProps)
        {
            if (_gameStateProvider.CurrentState != GameStates.PlayingTheGame)
                return false;

            return _currentMapProvider.CurrentMap.Tiles[renderProps.MapY, renderProps.MapX] == TileSpec.Jump
                || (renderProps.IsActing(CharacterActionState.Walking) && _currentMapProvider.CurrentMap.Tiles[renderProps.GetDestinationY(), renderProps.GetDestinationX()] == TileSpec.Jump);
        }

        private int GetSteppingStoneOffset(CharacterRenderProperties renderProps)
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

        private void CheckForDead()
        {
            if (_character == _characterProvider.MainCharacter && _lastIsDead != _character.RenderProperties.IsDead)
            {
                _lastIsDead = _character.RenderProperties.IsDead;
                if (_lastIsDead)
                {
                    _sfxPlayer.PlaySfx(SoundEffectID.Dead);
                }
            }
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

        public void ShowSpellAnimation(int spellGraphic)
        {
            _effectRenderer.PlayEffect(EffectType.Spell, spellGraphic);
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
        public void StopShout()
        {
            _nameLabel.Visible = false;
            _nameLabel.Text = _character.Name;
            _nameLabel.ForeColor = Color.White;
            _nameLabel.BlinkRate = null;
            _shoutName = string.Empty;
            _spellCastTime = null;
        }

        public void ShowDamageCounter(int damage, int percentHealth, bool isHeal)
        {
            if (isHeal)
                _healthBarRenderer.SetHealth(damage, percentHealth);
            else
                _healthBarRenderer.SetDamage(damage.SomeWhen(d => d > 0), percentHealth);
        }

        public void ShowChatBubble(string message, bool isGroupChat)
        {
            _chatBubble.Value.SetMessage(message, isGroupChat);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _outline?.Dispose();

                if (Game != null && Game.Components != null && Game.Components.Contains(_nameLabel))
                    Game.Components.Remove(_nameLabel);
                _nameLabel?.Dispose();

                if (_chatBubble.IsValueCreated)
                    _chatBubble.Value?.Dispose();

                _sb?.Dispose();

                lock(_rt_locker_)
                    _charRenderTarget?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
