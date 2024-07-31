using EndlessClient.Rendering.Metadata.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public interface ICharacterPropertyRenderer
    {
        bool CanRender { get; }

        float LayerDepth { get; set; }

        void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea, WeaponMetadata weaponMetadata);
    }
}