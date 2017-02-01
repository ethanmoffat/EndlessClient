// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Old;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.StatusBars
{
    public class TNLStatusBar : BaseStatusBar
    {
        public TNLStatusBar()
        {
            DrawLocation = new Vector2(430, 0);
            drawArea = new Rectangle((int)DrawLocation.X, (int)DrawLocation.Y, m_elemSourceRect.Width, m_elemSourceRect.Height);
            m_elemSourceRect = new Rectangle(m_elemSourceRect.Width * 3 - 1, 0, m_elemSourceRect.Width + 1, m_elemSourceRect.Height);
        }

        protected override void UpdateLabelText()
        {
            m_label.Text = $"{OldWorld.Instance.exp_table[m_stats.Level + 1] - m_stats.Experience}";
        }

        protected override void DrawStatusBar()
        {
            int thisLevel = OldWorld.Instance.exp_table[m_stats.Level];
            int nextLevel = OldWorld.Instance.exp_table[m_stats.Level + 1];
            int srcWidth = 25 + (int)Math.Round(((m_stats.Experience - thisLevel) / (double)(nextLevel - thisLevel)) * 79);
            Rectangle maskSrc = new Rectangle(m_elemSourceRect.X, m_elemSourceRect.Height, srcWidth, m_elemSourceRect.Height);

            SpriteBatch.Begin();
            SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), m_elemSourceRect, Color.White);
            SpriteBatch.Draw(m_textSheet, new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y), maskSrc, Color.White);
            SpriteBatch.End();
        }
    }
}
