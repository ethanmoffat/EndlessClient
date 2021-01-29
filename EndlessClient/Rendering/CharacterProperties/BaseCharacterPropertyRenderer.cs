using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public abstract class BaseCharacterPropertyRenderer : ICharacterPropertyRenderer
    {
        protected readonly ICharacterRenderProperties _renderProperties;

        public abstract bool CanRender { get; }

        public float LayerDepth { get; set; }

        protected virtual bool ShouldFlip => _renderProperties.IsFacing(EODirection.Up, EODirection.Right);

        protected BaseCharacterPropertyRenderer(ICharacterRenderProperties renderProperties)
        {
            _renderProperties = renderProperties;
            LayerDepth = 1.0f;
        }

        public abstract void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea);

        protected virtual void Render(SpriteBatch spriteBatch, ISpriteSheet sheet, Vector2 drawLoc, int alpha = 255)
        {
            spriteBatch.Draw(sheet.SheetTexture, drawLoc, sheet.SourceRectangle, Color.FromNonPremultiplied(255, 255, 255, alpha), 0.0f, Vector2.Zero, 1.0f,
                             ShouldFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                             LayerDepth);
        }
    }
}
