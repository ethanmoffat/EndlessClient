using EndlessClient.HUD.Panels;
using EOLib.Domain.Chat;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Chat
{
    public interface IChatRenderable
    {
        int DisplayIndex { get; set; }

        ChatData Data { get; }

        void Render(IHudPanel parentPanel, SpriteBatch spriteBatch, SpriteFont chatFont);
    }
}