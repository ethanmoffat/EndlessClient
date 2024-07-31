using EndlessClient.HUD.Panels;
using EOLib.Domain.Chat;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace EndlessClient.Rendering.Chat
{
    public interface IChatRenderable
    {
        int DisplayIndex { get; set; }

        ChatData Data { get; }

        void Render(IHudPanel parentPanel, SpriteBatch spriteBatch, BitmapFont chatFont);
    }
}