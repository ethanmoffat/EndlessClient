// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.HUD.Panels;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
    public class ChatRenderer : IChatRenderer
    {
        private struct ChatData
        {
            public int Index { get; private set; }

            public ChatType Type { get; private set; }

            public string Who { get; private set; }

            public string Message { get; private set; }

            private readonly ChatColor _chatColor;

            public Color Color { get { return GetColor(_chatColor); } }

            public ChatData(int index,
                            string who,
                            string message,
                            ChatType type = ChatType.None,
                            ChatColor color = ChatColor.Default)
                : this()
            {
                if (who == null)
                    who = "";
                else if (who.Length >= 1)
                    who = char.ToUpper(who[0]) + who.Substring(1).ToLower();

                if (message == null)
                    message = "";

                Index = index;
                Type = type;
                Who = who;
                Message = message;
                _chatColor = color;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ChatData)) return false;
                var other = (ChatData) obj;

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

        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        private readonly SpriteBatch _sb;

        public ChatRenderer(IGraphicsDeviceProvider graphicsDeviceProvider,
                            INativeGraphicsManager nativeGraphicsManager)
        {
            _graphicsDeviceProvider = graphicsDeviceProvider;
            _nativeGraphicsManager = nativeGraphicsManager;

            _sb = new SpriteBatch(_graphicsDeviceProvider.GraphicsDevice);
        }

        public void RenderNews(IReadOnlyList<string> newsText, int scrollOffset, int linesToRender)
        {
            //1. split text into chat strings
            //2. convert chat strings into ChatData
            //3. call DrawChat with chat data
        }

        private void DrawChat(IReadOnlyList<ChatData> chatData)
        {
            _sb.Begin();

            _sb.End();
        }

        private Texture2D GetChatIconGraphic(ChatType type)
        {
            var ret = new Texture2D(_graphicsDeviceProvider.GraphicsDevice, 13, 13);
            if (type == ChatType.None)
                return ret;

            var data = new Color[ret.Width*ret.Height];
            
            var texture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true);
            texture.GetData(0, new Rectangle(0, (int)type * 13, 13, 13), data, 0, data.Length);
            
            ret.SetData(data);

            return ret;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ChatRenderer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _sb.Dispose();
        }
    }
}
