using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Chat
{
    public interface IChatRenderable
    {
        int DisplayIndex { get; }

        void Render(SpriteBatch spriteBatch, SpriteFont chatFont, INativeGraphicsManager nativeGraphicsManager);

        void SetDisplayIndex(int newIndex);
    }
}