// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.HUD.Chat;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChatType = EndlessClient.HUD.Chat.ChatType;

namespace EndlessClient.Rendering.Chat
{
    public class ChatRenderable : IChatRenderable
    {
        private const int ICON_GRAPHIC_X_OFF = 3;
        private const int CHAT_MESSAGE_X_OFF = 20;
        private static readonly Vector2 TOP_LEFT = new Vector2(102, 330);

        protected virtual int HeaderYOffset { get { return 3; } }

        public int Index { get; private set; }

        public ChatType Type { get; private set; }

        public string Who { get; private set; }

        public string Message { get; private set; }

        private readonly ChatColor _chatColor;

        public Color Color { get { return GetColor(_chatColor); } }

        public ChatRenderable(int index,
                              string who,
                              string message,
                              ChatType type = ChatType.None,
                              ChatColor color = ChatColor.Default)
        {
            if (who == null)
                who = "";
            else if (who.Length >= 1)
                who = Char.ToUpper(who[0]) + who.Substring(1).ToLower();

            if (message == null)
                message = "";

            Index = index;
            Type = type;
            Who = who;
            Message = message;
            _chatColor = color;
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
                   other.Type == Type &&
                   other.Who == Who &&
                   other._chatColor == _chatColor;
        }

        public override int GetHashCode()
        {
            var hash = 397 ^ Index;
            hash = (hash*397) ^ (int) Type;
            hash = (hash*397) ^ (int) _chatColor;
            hash = (hash*397) ^ Who.GetHashCode();
            return hash;
        }

        public void Render(SpriteBatch spriteBatch, SpriteFont chatFont, INativeGraphicsManager nativeGraphicsManager)
        {
            spriteBatch.Begin();

            var pos = TOP_LEFT + new Vector2(0, Index*13);
            spriteBatch.Draw(GetChatIconGraphic(Type, nativeGraphicsManager, spriteBatch.GraphicsDevice),
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

        private Texture2D GetChatIconGraphic(ChatType type, INativeGraphicsManager graphicsManager, GraphicsDevice graphicsDevice)
        {
            var ret = new Texture2D(graphicsDevice, 13, 13);
            if (type == ChatType.None)
                return ret;

            var data = new Color[ret.Width * ret.Height];

            var texture = graphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true);
            texture.GetData(0, new Rectangle(0, (int)type * 13, 13, 13), data, 0, data.Length);

            ret.SetData(data);

            return ret;
        }

        private static Color GetColor(ChatColor chatColor)
        {
            switch (chatColor)
            {
                case ChatColor.Default: return Color.Black;
                case ChatColor.Error: return Color.FromNonPremultiplied(0x7d, 0x0a, 0x0a, 0xff);
                case ChatColor.PM: return Color.FromNonPremultiplied(0x5a, 0x3c, 0x00, 0xff);
                case ChatColor.Server: return Color.FromNonPremultiplied(0xe6, 0xd2, 0xc8, 0xff);
                case ChatColor.ServerGlobal: return ColorConstants.LightYellowText;
                case ChatColor.Admin: return Color.FromNonPremultiplied(0xc8, 0xaa, 0x96, 0xff);
                default: throw new ArgumentOutOfRangeException("chatColor", chatColor, "Unrecognized chat color");
            }
        }
    }

    public class NewsChatRenderable : ChatRenderable
    {
        protected override int HeaderYOffset { get { return 23; } }

        public NewsChatRenderable(int index,
                                  string who,
                                  string message,
                                  ChatType type = ChatType.None,
                                  ChatColor color = ChatColor.Default)
            : base(index, who, message, type, color)
        {
        }
    }
}