// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.StatusBars
{
    public class HPStatusBar : BaseStatusBar
    {
        public HPStatusBar()
        {
            DrawLocation = new Vector2(100, 0);
            drawArea = new Rectangle((int)DrawLocation.X, (int)DrawLocation.Y, m_elemSourceRect.Width, m_elemSourceRect.Height);
        }

        protected override void UpdateLabelText()
        {
            m_label.Text = $"{m_stats.HP}/{m_stats.MaxHP}";
        }

        protected override void DrawStatusBar()
        {
            int srcWidth = 25 + (int)Math.Round((m_stats.HP / (double)m_stats.MaxHP) * 79);
            Rectangle maskSrc = new Rectangle(m_elemSourceRect.X, m_elemSourceRect.Height, srcWidth, m_elemSourceRect.Height);

            SpriteBatch.Begin();
            SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), m_elemSourceRect, Color.White);
            SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), maskSrc, Color.White);
            SpriteBatch.End();
        }
    }
}
