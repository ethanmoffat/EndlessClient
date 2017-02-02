// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Rendering.Chat;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Rendering
{
    //todo: handle group chat color
    public class ChatBubble : IChatBubble
    {
        private readonly IHaveChatBubble _referenceRenderer;
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;

        private readonly XNALabel _textLabel;
        private readonly SpriteBatch _spriteBatch;

        private Vector2 _drawLocation;
        private Optional<DateTime> _startTime;

        public ChatBubble(IHaveChatBubble referenceRenderer,
                          IChatBubbleTextureProvider chatBubbleTextureProvider,
                          IGraphicsDeviceProvider graphicsDeviceProvider)
        {
            _referenceRenderer = referenceRenderer;
            _chatBubbleTextureProvider = chatBubbleTextureProvider;
            _spriteBatch = new SpriteBatch(graphicsDeviceProvider.GraphicsDevice);

            _textLabel = new XNALabel(Constants.FontSize08pt5)
            {
                Visible = true,
                TextWidth = 165,
                TextAlign = LabelAlignment.MiddleCenter,
                ForeColor = Color.Black,
                AutoSize = true,
                Text = ""
            };
            _textLabel.Initialize(); //todo: figure out if this needs to be removed from game components

            _drawLocation = Vector2.Zero;
            _startTime = Optional<DateTime>.Empty;

            _setLabelDrawLoc();
        }

        public void HideBubble()
        {
            _textLabel.Text = "";
            _textLabel.Visible = false;
        }

        public void SetMessage(string message)
        {
            _textLabel.Text = message;
            _textLabel.Visible = true;

            _startTime = DateTime.Now;
        }

        private void _setLabelDrawLoc()
        {
            var extra = _chatBubbleTextureProvider.ChatBubbleTextures[ChatBubbleTexture.MiddleLeft].Width;
            _textLabel.DrawPosition = new Vector2(
                _referenceRenderer.DrawArea.X + _referenceRenderer.DrawArea.Width/2.0f - _textLabel.ActualWidth/2.0f + extra,
                _referenceRenderer.DrawArea.Y - _textLabel.ActualHeight - 5);
        }

        //public override void Update(GameTime gameTime)
        //{
        //    if (!Visible || m_ref == null || !s_textsLoaded || m_label == null)
        //        return;

        //    _setLabelDrawLoc();
        //    m_drawLoc = m_label.DrawLocation - new Vector2(s_textures[TL].Width, s_textures[TL].Height);

        //    //This replaces the goAway timer.
        //    if (m_startTime.HasValue && (DateTime.Now - m_startTime.Value).TotalMilliseconds > Constants.ChatBubbleTimeout)
        //    {
        //        Visible = false;
        //        m_label.Visible = false;
        //        m_startTime = null;
        //    }

        //    base.Update(gameTime);
        //}

        //public override void Draw(GameTime gameTime)
        //{
        //    if (!s_textsLoaded || !Visible) return;
        //    int xCov = s_textures[TL].Width;
        //    int yCov = s_textures[TL].Height;
        //    if (m_sb == null) return;

        //    Color col = m_useGroupChatColor ? Color.Tan : Color.FromNonPremultiplied(255, 255, 255, 232);

        //    m_sb.Begin();

        //    //top row
        //    m_sb.Draw(s_textures[TL], m_drawLoc, col);
        //    int xCur;
        //    for (xCur = xCov; xCur < m_label.ActualWidth + 6; xCur += s_textures[TM].Width)
        //    {
        //        m_sb.Draw(s_textures[TM], m_drawLoc + new Vector2(xCur, 0), col);
        //    }
        //    m_sb.Draw(s_textures[TR], m_drawLoc + new Vector2(xCur, 0), col);

        //    //middle area
        //    int y;
        //    for (y = yCov; y < m_label.ActualHeight; y += s_textures[ML].Height)
        //    {
        //        m_sb.Draw(s_textures[ML], m_drawLoc + new Vector2(0, y), col);
        //        int x;
        //        for (x = xCov; x < xCur; x += s_textures[MM].Width)
        //        {
        //            m_sb.Draw(s_textures[MM], m_drawLoc + new Vector2(x, y), col);
        //        }
        //        m_sb.Draw(s_textures[MR], m_drawLoc + new Vector2(xCur, y), col);
        //    }

        //    //bottom row
        //    m_sb.Draw(s_textures[RL], m_drawLoc + new Vector2(0, y), col);
        //    int x2;
        //    for (x2 = xCov; x2 < xCur; x2 += s_textures[RM].Width)
        //    {
        //        m_sb.Draw(s_textures[RM], m_drawLoc + new Vector2(x2, y), col);
        //    }
        //    m_sb.Draw(s_textures[RR], m_drawLoc + new Vector2(x2, y), col);
        //    y += s_textures[RM].Height;
        //    m_sb.Draw(s_textures[NUB], m_drawLoc + new Vector2((x2 + s_textures[RR].Width - s_textures[NUB].Width) / 2f, y - 1), col);

        //    try
        //    {
        //        m_sb.End();
        //    }
        //    catch (ObjectDisposedException) { }
        //    base.Draw(gameTime);
        //}

        ~ChatBubble()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _spriteBatch.Dispose();
                _textLabel.Dispose();
            }
        }
    }

    public interface IChatBubble : IDisposable
    {
        void SetMessage(string message);

        void HideBubble();
    }
}
