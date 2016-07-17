// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Graphics;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
    [Flags]
    public enum StatusIconType
    {
        Weapon = 1,
        Shield = 2,
        Spell = 4,
        PK = 8
    }

    public sealed class PlayerStatusIconRenderer : DrawableGameComponent
    {
        private const int ABS_X_OFFSET = 14;
        private const int ABS_Y_OFFSET = 285;

        private static readonly StatusIconType[] _orderedValues =
        {
            StatusIconType.PK,
            StatusIconType.Spell,
            StatusIconType.Weapon,
            StatusIconType.Shield
        };

        private readonly Texture2D _statusIcons;
        private readonly int _widthDelta;
        private readonly int _heightDelta;
        private readonly Color _renderColor;
        private readonly SpriteBatch _sb;

        private StatusIconType _icons;

        public PlayerStatusIconRenderer(EOGame game)
            : base(game)
        {
            _statusIcons = EOGame.Instance.GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 46, true);
            _widthDelta = _statusIcons.Width / 4;
            _heightDelta = _statusIcons.Height / 2;
            _renderColor = Color.FromNonPremultiplied(0x9e, 0x9f, 0x9e, 0xff);
            _sb = new SpriteBatch(game.GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Game.IsActive || !Visible) return;

            var c = OldWorld.Instance.MainPlayer.ActiveCharacter;
            var m = OldWorld.Instance.ActiveMapRenderer.MapRef;
            
            _icons = 0;
            if (m.Properties.PKAvailable)
                _icons |= StatusIconType.PK;
            if(c.SelectedSpell > 0)
                _icons |= StatusIconType.Spell;
            if(c.PaperDoll[(int)EquipLocation.Weapon] > 0)
                _icons |= StatusIconType.Weapon;
            if(c.PaperDoll[(int)EquipLocation.Shield] > 0)
                _icons |= StatusIconType.Shield;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gt)
        {
            if (!Visible) return;

            _sb.Begin();
            int extraOffset = 0; //changes based on presence or absence of other icons
            foreach (var icon in _orderedValues)
            {
                if ((_icons & icon) > 0)
                {
                    _sb.Draw(_statusIcons, GetTargetPosition(extraOffset), GetSourceRectangle(icon), _renderColor);
                    extraOffset += 24;
                }
            }
            _sb.End();
        }

        private Vector2 GetTargetPosition(int offset)
        {
            return new Vector2(offset + ABS_X_OFFSET, ABS_Y_OFFSET);
        }

        private Rectangle GetSourceRectangle(StatusIconType type)
        {
            //convert from power of two 'flag' value to base 10 index
            var index = (int)Math.Log((int) type, 2);

            var xOffset = _widthDelta*index;
            return new Rectangle(xOffset, 0, _widthDelta, _heightDelta);
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sb.Dispose();
            }
        }

        #endregion
    }
}
