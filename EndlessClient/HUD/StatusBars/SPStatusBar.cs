// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.StatusBars
{
    public class SPStatusBar : BaseStatusBar
    {
        public SPStatusBar()
        {
            DrawLocation = new Vector2(320, 0);
            drawArea = new Rectangle((int)DrawLocation.X, (int)DrawLocation.Y, m_elemSourceRect.Width, m_elemSourceRect.Height);
            m_elemSourceRect.Offset(m_elemSourceRect.Width * 2, 0);
        }

        protected override void UpdateLabelText()
        {
            m_label.Text = $"{m_stats.SP}/{m_stats.MaxSP}";
        }

        protected override void DrawStatusBar()
        {
            int srcWidth = 25 + (int)Math.Round((m_stats.SP / (double)m_stats.MaxSP) * 79);
            Rectangle maskSrc = new Rectangle(m_elemSourceRect.X, m_elemSourceRect.Height, srcWidth, m_elemSourceRect.Height);

            SpriteBatch.Begin();
            SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), m_elemSourceRect, Color.White);
            SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), maskSrc, Color.White);
            SpriteBatch.End();
        }
    }
}
