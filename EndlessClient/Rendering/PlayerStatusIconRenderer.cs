using EndlessClient.HUD.Spells;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using XNAControls;

namespace EndlessClient.Rendering
{
    public sealed class PlayerStatusIconRenderer : XNAControl
    {
        [Flags]
        private enum StatusIconType
        {
            Weapon = 1,
            Shield = 2,
            Spell = 4,
            PK = 8
        }

        private static readonly StatusIconType[] _orderedValues =
        {
            StatusIconType.PK,
            StatusIconType.Spell,
            StatusIconType.Weapon,
            StatusIconType.Shield
        };

        private readonly ICharacterProvider _characterProvider;
        private readonly ISpellSlotDataProvider _spellSlotDataProvider;
        private readonly ICurrentMapProvider _currentMapProvider;

        private readonly Texture2D _statusIcons;

        private StatusIconType _icons;

        public PlayerStatusIconRenderer(INativeGraphicsManager nativeGraphicsManager,
                                        ICharacterProvider characterProvider,
                                        ISpellSlotDataProvider spellSlotDataProvider,
                                        ICurrentMapProvider currentMapProvider,
                                        IClientWindowSizeProvider clientWindowSizeProvider)
        {
            _characterProvider = characterProvider;
            _spellSlotDataProvider = spellSlotDataProvider;
            _currentMapProvider = currentMapProvider;

            _statusIcons = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 46, true);

            DrawPosition = new Vector2(14, clientWindowSizeProvider.Height - _statusIcons.Height - 3);
            clientWindowSizeProvider.GameWindowSizeChanged += (o, e) => DrawPosition = new Vector2(14, clientWindowSizeProvider.Height - _statusIcons.Height - 3);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            var c = _characterProvider.MainCharacter;

            _icons = 0;
            if (_currentMapProvider.CurrentMap.Properties.PKAvailable)
                _icons |= StatusIconType.PK;
            if (_spellSlotDataProvider.SelectedSpellSlot.HasValue)
                _icons |= StatusIconType.Spell;
            if (c.RenderProperties.WeaponGraphic > 0)
                _icons |= StatusIconType.Weapon;
            if (c.RenderProperties.ShieldGraphic > 0)
                _icons |= StatusIconType.Shield;

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gt)
        {
            _spriteBatch.Begin();

            int extraOffset = 0;
            foreach (var icon in _orderedValues)
            {
                if ((_icons & icon) > 0)
                {
                    _spriteBatch.Draw(_statusIcons, DrawPositionWithParentOffset + new Vector2(extraOffset, 0), GetSourceRectangle(icon), Color.FromNonPremultiplied(0x9e, 0x9f, 0x9e, 0xff));
                    extraOffset += 24;
                }
            }

            _spriteBatch.End();

            base.OnDrawControl(gt);
        }

        private Rectangle GetSourceRectangle(StatusIconType type)
        {
            var widthDelta = _statusIcons.Width / 4;
            var heightDelta = _statusIcons.Height / 2;

            //convert from power of two 'flag' value to base 10 index
            var index = (int)Math.Log((int)type, 2);

            var xOffset = widthDelta * index;
            return new Rectangle(xOffset, 0, widthDelta, heightDelta);
        }
    }
}