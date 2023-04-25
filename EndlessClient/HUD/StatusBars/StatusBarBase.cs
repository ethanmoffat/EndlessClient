using System;
using EndlessClient.Input;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using XNAControls;

namespace EndlessClient.HUD.StatusBars
{
    public abstract class StatusBarBase : XNAControl
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICharacterProvider _characterProvider;
        private readonly IUserInputRepository _userInputRepository;

        protected readonly XNALabel _label;
        protected readonly Texture2D _texture;

        protected CharacterStats Stats => _characterProvider.MainCharacter.Stats;
        protected Rectangle _sourceRectangleArea;

        private Option<DateTime> _labelShowTime;

        public event Action StatusBarClicked;
        public event Action StatusBarClosed;

        protected StatusBarBase(INativeGraphicsManager nativeGraphicsManager,
                                ICharacterProvider characterProvider,
                                IUserInputRepository userInputRepository)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterProvider = characterProvider;
            _userInputRepository = userInputRepository;

            _texture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 58, true);

            _label = new XNALabel(Constants.FontSize08)
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                DrawPosition = new Vector2(6, 15),
                ForeColor = ColorConstants.LightGrayText,
                Visible = false
            };
            _label.SetParentControl(this);

            _sourceRectangleArea = new Rectangle(0, 0, 110, 14);
        }

        protected abstract void UpdateLabelText();
        protected abstract void DrawStatusBar();

        public override void Initialize()
        {
            _label.Initialize();
            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            _labelShowTime.MatchSome(x =>
            {
                UpdateLabelText();

                if ((DateTime.Now - x).TotalSeconds >= 4)
                {
                    _label.Visible = false;
                    _labelShowTime = Option.None<DateTime>();

                    StatusBarClosed?.Invoke();
                }
            });

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            DrawStatusBar();

            if (_labelShowTime.HasValue)
            {
                var dest = new Vector2(DrawAreaWithParentOffset.X,
                                       DrawAreaWithParentOffset.Y + _sourceRectangleArea.Height - 3);

                _spriteBatch.Begin();
                _spriteBatch.Draw(_texture, dest, DropDownSourceRectangle, Color.White);
                _spriteBatch.End();
            }

            base.OnDrawControl(gameTime);
        }

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            // eat this mouse click so that other game elements don't attempt to use it
            _userInputRepository.PreviousMouseState = _userInputRepository.CurrentMouseState;

            _label.Visible = !_label.Visible;
            _labelShowTime = _label.SomeWhen(x => x.Visible).Map(_ => DateTime.Now);

            StatusBarClicked?.Invoke();

            return true;
        }

        /// <summary>
        /// Source rectangle for the drop-down box in the texture sprite sheet (shown when control is clicked)
        /// </summary>
        private static Rectangle DropDownSourceRectangle => new Rectangle(220, 30, 110, 21);
    }
}
