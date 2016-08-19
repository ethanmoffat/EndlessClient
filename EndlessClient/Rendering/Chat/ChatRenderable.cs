// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.HUD.Chat;
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

        protected virtual int HeaderYOffset { get { return 3; } }

        public int Index { get; private set; }

        public ChatIcon Icon { get; private set; }

        public string Who { get; private set; }

        public string Message { get; private set; }

        public ChatColor ChatColor { get; private set; }

        public Color Color { get { return ChatColor.ToColor(); } }

        public ChatRenderable(int index,
                              string who,
                              string message,
                              ChatIcon icon = ChatIcon.None,
                              ChatColor color = ChatColor.Default)
        {
            if (who == null)
                who = "";
            else if (who.Length >= 1)
                who = Char.ToUpper(who[0]) + who.Substring(1).ToLower();

            if (message == null)
                message = "";

            Index = index;
            Icon = icon;
            Who = who;
            Message = message;
            ChatColor = color;
        }

        public void UpdateIndex(int newIndex)
        {
            Index = newIndex;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChatRenderable)) return false;
            var other = (ChatRenderable) obj;

            return other.Index == Index &&
                   other.Icon == Icon &&
                   other.Who == Who &&
                   other.ChatColor == ChatColor;
        }

        public override int GetHashCode()
        {
            var hash = 397 ^ Index.GetHashCode();
            hash = (hash*397) ^ Icon.GetHashCode();
            hash = (hash*397) ^ ChatColor.GetHashCode();
            hash = (hash*397) ^ Who.GetHashCode();
            return hash;
        }

        public void Render(SpriteBatch spriteBatch, SpriteFont chatFont, INativeGraphicsManager nativeGraphicsManager)
        {
            spriteBatch.Begin();

            var pos = TOP_LEFT + new Vector2(0, Index*13);
            spriteBatch.Draw(GetChatIconGraphic(Icon, nativeGraphicsManager, spriteBatch.GraphicsDevice),
                             new Vector2(pos.X + ICON_GRAPHIC_X_OFF, pos.Y + HeaderYOffset),
                             Color.White);

            string strToDraw;
            if (string.IsNullOrEmpty(Who))
                strToDraw = Message;
            else
                strToDraw = Who + "  " + Message;

            spriteBatch.DrawString(chatFont,
                                   strToDraw,
                                   new Vector2(pos.X + CHAT_MESSAGE_X_OFF, pos.Y + HeaderYOffset),
                                   Color);

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

        public NewsChatRenderable(int index,
                                  string who,
                                  string message,
                                  ChatIcon icon = ChatIcon.None,
                                  ChatColor color = ChatColor.Default)
            : base(index, who, message, icon, color)
        {
        }
    }
}