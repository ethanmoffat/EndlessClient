using EOLib.Domain.Chat;
using EOLib.Extensions;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Optional.Unsafe;

namespace EndlessClient.Rendering.Chat
{
    public class ChatRenderable : IChatRenderable
    {
        private const int ICON_GRAPHIC_X_OFF = 3;
        private const int CHAT_MESSAGE_X_OFF = 20;
        private static readonly Vector2 TOP_LEFT = new Vector2(102, 330);

        private readonly ChatData _data;
        private readonly string _partialMessage;

        protected virtual int HeaderYOffset => 3;

        public int DisplayIndex { get; private set; }

        public ChatRenderable(int displayIndex,
                              ChatData data,
                              string partialMessage = null)
        {
            DisplayIndex = displayIndex;
            _data = data;
            _partialMessage = partialMessage;
        }

        public void SetDisplayIndex(int newIndex)
        {
            DisplayIndex = newIndex;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChatRenderable)) return false;
            var other = (ChatRenderable) obj;

            return other._data.Equals(_data)
                && other._partialMessage.Equals(_partialMessage);
        }

        public override int GetHashCode()
        {
            var hash = 397 ^ _data.GetHashCode();
            hash = (hash*397) ^ DisplayIndex.GetHashCode();
            hash = (hash*397) ^ _partialMessage.GetHashCode();
            return hash;
        }

        public void Render(SpriteBatch spriteBatch, SpriteFont chatFont, INativeGraphicsManager nativeGraphicsManager)
        {
            spriteBatch.Begin();

            var pos = TOP_LEFT + new Vector2(0, DisplayIndex*13);
            spriteBatch.Draw(nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true),
                             new Vector2(pos.X + ICON_GRAPHIC_X_OFF, pos.Y + HeaderYOffset),
                             GetChatIconRectangle(_data.Icon),
                             Color.White);

            string strToDraw;
            if (string.IsNullOrEmpty(_data.Who))
                strToDraw = _partialMessage;
            else
                strToDraw = _data.Who + "  " + _partialMessage;

            spriteBatch.DrawString(chatFont,
                                   strToDraw,
                                   new Vector2(pos.X + CHAT_MESSAGE_X_OFF, pos.Y + HeaderYOffset),
                                   _data.ChatColor.ToColor());

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

        public NewsChatRenderable(int displayIndex, ChatData data, string partialMessage)
            : base(displayIndex, data, partialMessage)
        {
        }
    }
}