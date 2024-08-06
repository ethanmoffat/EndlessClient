using EndlessClient.HUD.Panels;
using EOLib.Domain.Chat;
using EOLib.Extensions;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Optional.Unsafe;

namespace EndlessClient.Rendering.Chat
{
    public class ChatRenderable : IChatRenderable
    {
        private const int ICON_GRAPHIC_X_OFF = 3;
        private const int CHAT_MESSAGE_X_OFF = 20;

        private readonly INativeGraphicsManager _nativeGraphicsManager;

        private readonly string _partialMessage;

        protected virtual int HeaderYOffset => 3;

        public int DisplayIndex { get; set; }

        public ChatData Data { get; private set; }

        public ChatRenderable(INativeGraphicsManager nativeGraphicsManager,
                              int displayIndex,
                              ChatData data,
                              string partialMessage = null)
        {
            _nativeGraphicsManager = nativeGraphicsManager;

            DisplayIndex = displayIndex;
            Data = data;
            _partialMessage = partialMessage;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChatRenderable)) return false;
            var other = (ChatRenderable)obj;

            return other.Data.Equals(Data)
                && other._partialMessage.Equals(_partialMessage);
        }

        public override int GetHashCode()
        {
            var hash = 397 ^ Data.GetHashCode();
            hash = (hash * 397) ^ DisplayIndex.GetHashCode();
            hash = (hash * 397) ^ _partialMessage.GetHashCode();
            return hash;
        }

        public void Render(IHudPanel parentPanel, SpriteBatch spriteBatch, BitmapFont chatFont)
        {
            spriteBatch.Begin();

            var pos = parentPanel.DrawPosition + new Vector2(0, DisplayIndex * 13);
            spriteBatch.Draw(_nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true),
                             new Vector2(pos.X + ICON_GRAPHIC_X_OFF, pos.Y + HeaderYOffset),
                             GetChatIconRectangle(Data.Icon),
                             Color.White);

            string strToDraw;
            if (string.IsNullOrEmpty(Data.Who))
                strToDraw = _partialMessage;
            else
                strToDraw = Data.Who + "  " + _partialMessage;

            spriteBatch.DrawString(chatFont,
                                   strToDraw,
                                   new Vector2(pos.X + CHAT_MESSAGE_X_OFF, pos.Y + HeaderYOffset),
                                   Data.ChatColor.ToColor());

            spriteBatch.End();
        }

        private static Rectangle? GetChatIconRectangle(ChatIcon icon)
        {
            var (x, y, width, height) = icon.GetChatIconRectangleBounds().ValueOrDefault();
            return new Rectangle(x, y, width, height);
        }
    }

    public class NewsChatRenderable : ChatRenderable
    {
        protected override int HeaderYOffset => 23;

        public NewsChatRenderable(INativeGraphicsManager nativeGraphicsManager, int displayIndex, ChatData data, string partialMessage)
            : base(nativeGraphicsManager, displayIndex, data, partialMessage)
        {
        }
    }
}
