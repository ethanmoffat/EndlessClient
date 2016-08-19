// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Chat;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Chat
{
    public class ChatRenderable : IChatRenderable
    {
        private const int ICON_GRAPHIC_X_OFF = 3;
        private const int CHAT_MESSAGE_X_OFF = 20;
        private static readonly Vector2 TOP_LEFT = new Vector2(102, 330);

        private readonly ChatData _data;
        private readonly string _partialMessage;

        protected virtual int HeaderYOffset { get { return 3; } }

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
            spriteBatch.Draw(GetChatIconGraphic(_data.Icon, nativeGraphicsManager, spriteBatch.GraphicsDevice),
                             new Vector2(pos.X + ICON_GRAPHIC_X_OFF, pos.Y + HeaderYOffset),
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

        private Texture2D GetChatIconGraphic(ChatIcon icon, INativeGraphicsManager graphicsManager, GraphicsDevice graphicsDevice)
        {
            var ret = new Texture2D(graphicsDevice, 13, 13);
            if (icon == ChatIcon.None)
                return ret;

            var data = new Color[ret.Width * ret.Height];

            var texture = graphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true);
            texture.GetData(0, new Rectangle(0, (int)icon * 13, 13, 13), data, 0, data.Length);

            ret.SetData(data);

            return ret;
        }
    }

    public class NewsChatRenderable : ChatRenderable
    {
        protected override int HeaderYOffset { get { return 23; } }

        public NewsChatRenderable(int displayIndex, ChatData data, string partialMessage)
            : base(displayIndex, data, partialMessage)
        {
        }
    }
}