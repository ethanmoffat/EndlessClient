using System;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Optional;
using XNAControls;

namespace EndlessClient.Rendering.Chat
{
    //todo: clear message when IHaveChatBubble dies
    public class ChatBubble : DrawableGameComponent, IChatBubble
    {
        private readonly IMapActor _parent;
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly SpriteBatch _spriteBatch;

        private readonly XNALabel _textLabel;

        private bool _isGroupChat;
        private Vector2 _drawLocation;
        private Option<DateTime> _startTime;

        public ChatBubble(IMapActor referenceRenderer,
                          IChatBubbleTextureProvider chatBubbleTextureProvider,
                          IEndlessGameProvider gameProvider,
                          IConfigurationProvider configurationProvider)
            : base((Game)gameProvider.Game)
        {
            _parent = referenceRenderer;
            _chatBubbleTextureProvider = chatBubbleTextureProvider;
            _configurationProvider = configurationProvider;
            _spriteBatch = new SpriteBatch(((Game)gameProvider.Game).GraphicsDevice);

            _textLabel = new XNALabel(Constants.FontSize08pt5)
            {
                Visible = false,
                TextWidth = 150,
                ForeColor = Color.Black,
                AutoSize = true,
                Text = string.Empty,
                DrawOrder = 30,
                KeepInClientWindowBounds = false,
            };

            _drawLocation = Vector2.Zero;
            _startTime = Option.None<DateTime>();

            DrawOrder = 29;
            Visible = false;
        }

        public override void Initialize()
        {
            _textLabel.Initialize();

            if (!_textLabel.Game.Components.Contains(_textLabel))
                _textLabel.Game.Components.Add(_textLabel);

            base.Initialize();
        }

        public void SetMessage(string message, bool isGroupChat)
        {
            if (!_configurationProvider.ShowChatBubbles)
                return;

            _isGroupChat = isGroupChat;
            _textLabel.Text = message;
            Visible = true;
            _textLabel.Visible = true;

            _startTime = Option.Some(DateTime.Now);
        }

        public override void Update(GameTime gameTime)
        {
            SetLabelDrawPosition();
            _drawLocation = _textLabel.DrawPosition - new Vector2(
                _chatBubbleTextureProvider.ChatBubbleTextures[ChatBubbleTexture.TopLeft].Width,
                _chatBubbleTextureProvider.ChatBubbleTextures[ChatBubbleTexture.TopLeft].Height);

            _startTime.MatchSome(st =>
            {
                if ((DateTime.Now - st).TotalMilliseconds > Constants.ChatBubbleTimeout)
                {
                    _textLabel.Visible = false;
                    Visible = false;
                    _startTime = Option.None<DateTime>();
                }
            });

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            var TL = GetTexture(ChatBubbleTexture.TopLeft);
            var TM = GetTexture(ChatBubbleTexture.TopMiddle);
            var TR = GetTexture(ChatBubbleTexture.TopRight);
            var ML = GetTexture(ChatBubbleTexture.MiddleLeft);
            var MM = GetTexture(ChatBubbleTexture.MiddleMiddle);
            var MR = GetTexture(ChatBubbleTexture.MiddleRight);
            var BL = GetTexture(ChatBubbleTexture.BottomLeft);
            var BM = GetTexture(ChatBubbleTexture.BottomMiddle);
            var BR = GetTexture(ChatBubbleTexture.BottomRight);
            var NUB = GetTexture(ChatBubbleTexture.Nubbin);

            var xCov = TL.Width;
            var yCov = TL.Height;
            
            var color = _isGroupChat ? Color.Tan : Color.FromNonPremultiplied(255, 255, 255, 232);

            _spriteBatch.Begin();

            //top row
            _spriteBatch.Draw(TL, _drawLocation, color);
            int xCur;
            for (xCur = xCov; xCur < _textLabel.ActualWidth + 6; xCur += TM.Width)
            {
                _spriteBatch.Draw(TM, _drawLocation + new Vector2(xCur, 0), color);
            }
            _spriteBatch.Draw(TR, _drawLocation + new Vector2(xCur, 0), color);

            //middle area
            int y;
            for (y = yCov; y < _textLabel.ActualHeight; y += ML.Height)
            {
                _spriteBatch.Draw(ML, _drawLocation + new Vector2(0, y), color);
                int x;
                for (x = xCov; x < xCur; x += MM.Width)
                {
                    _spriteBatch.Draw(MM, _drawLocation + new Vector2(x, y), color);
                }
                _spriteBatch.Draw(MR, _drawLocation + new Vector2(xCur, y), color);
            }

            //bottom row
            _spriteBatch.Draw(BL, _drawLocation + new Vector2(0, y), color);
            int x2;
            for (x2 = xCov; x2 < xCur; x2 += BM.Width)
            {
                _spriteBatch.Draw(BM, _drawLocation + new Vector2(x2, y), color);
            }
            _spriteBatch.Draw(BR, _drawLocation + new Vector2(x2, y), color);

            y += BM.Height;
            _spriteBatch.Draw(NUB, _drawLocation + new Vector2((x2 + BR.Width - NUB.Width)/2f, y - 1), color);

            _spriteBatch.End();
        }

        private void SetLabelDrawPosition()
        {
            _textLabel.DrawPosition = new Vector2(
                _parent.HorizontalCenter - _textLabel.ActualWidth / 2.0f,
                _parent.TopPixelWithOffset - _textLabel.ActualHeight - (GetTexture(ChatBubbleTexture.TopMiddle).Height * 5));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Game.Components != null && Game.Components.Contains(this))
                    Game.Components.Remove(this);

                _textLabel.Dispose();
            }
        }

        private Texture2D GetTexture(ChatBubbleTexture whichTexture) =>
            _chatBubbleTextureProvider.ChatBubbleTextures[whichTexture];
    }

    public interface IChatBubble : IDisposable
    {
        void SetMessage(string message, bool isGroupChat);
    }
}
