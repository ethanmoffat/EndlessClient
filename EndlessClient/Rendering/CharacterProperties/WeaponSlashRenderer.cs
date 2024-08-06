using EndlessClient.Rendering.Metadata.Models;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class WeaponSlashRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _slashSheet;

        public override bool CanRender => _slashSheet.HasTexture && _renderProperties.WeaponGraphic != 0 && _renderProperties.RenderAttackFrame == 2;

        protected override bool ShouldFlip => false;

        public WeaponSlashRenderer(CharacterRenderProperties renderProperties,
                                   ISpriteSheet slashSheet)
            : base(renderProperties)
        {
            _slashSheet = slashSheet;
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea, WeaponMetadata weaponMetadata)
        {
            if (weaponMetadata.Slash == null || weaponMetadata.Ranged)
                return;

            var offsets = GetOffsets(parentCharacterDrawArea) - new Vector2(0, _renderProperties.Gender);
            Render(spriteBatch, _slashSheet, parentCharacterDrawArea.Location.ToVector2() + offsets, 96);
        }

        private Vector2 GetOffsets(Rectangle parentCharacterDrawArea)
        {
            return _renderProperties.Direction switch
            {
                EODirection.Down => new Vector2(-30, 4),
                EODirection.Left => new Vector2(-34, -9),
                EODirection.Up => new Vector2(-6, -9),
                EODirection.Right => new Vector2(-10, 4),
                _ => Vector2.Zero
            };
        }
    }
}
