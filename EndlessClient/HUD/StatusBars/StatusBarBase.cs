// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.HUD.StatusBars
{
    public abstract class StatusBarBase : XNAControl
    {
        private readonly ICharacterProvider _characterProvider;

        protected readonly XNALabel _label;
        protected readonly Texture2D _texture;

        protected ICharacterStats Stats => _characterProvider.MainCharacter.Stats;
        protected Rectangle _sourceRectangleArea;

        private Optional<DateTime> _labelShowTime;

        protected StatusBarBase(INativeGraphicsManager nativeGraphicsManager,
                                ICharacterProvider characterProvider)
        {
            _characterProvider = characterProvider;
            _texture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 58, true);

            _label = new XNALabel(Constants.FontSize08)
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                DrawPosition = new Vector2(3, 15),
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
            if (MouseOver &&
                CurrentMouseState.LeftButton == ButtonState.Released &&
                PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                _label.Visible = !_label.Visible;
                _labelShowTime = _label.Visible
                    ? new Optional<DateTime>(DateTime.Now)
                    : Optional<DateTime>.Empty;
            }

            if (_labelShowTime.HasValue)
            {
                UpdateLabelText();

                if ((DateTime.Now - _labelShowTime).TotalSeconds >= 4)
                {
                    _label.Visible = false;
                    _labelShowTime = Optional<DateTime>.Empty;
                }
            }

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

        /// <summary>
        /// Source rectangle for the drop-down box in the texture sprite sheet (shown when control is clicked)
        /// </summary>
        private static Rectangle DropDownSourceRectangle => new Rectangle(220, 30, 110, 21);
    }
}
