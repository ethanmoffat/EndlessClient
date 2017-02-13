// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.StatusBars
{
    public class TPStatusBar : StatusBarBase
    {
        public TPStatusBar(INativeGraphicsManager nativeGraphicsManager,
                           ICharacterProvider characterProvider)
            : base(nativeGraphicsManager, characterProvider)
        {
            DrawArea = new Rectangle(210, 0, _sourceRectangleArea.Width, _sourceRectangleArea.Height);
            _sourceRectangleArea.Offset(_sourceRectangleArea.Width, 0);
        }

        protected override void UpdateLabelText()
        {
            _label.Text = $"{Stats[CharacterStat.TP]}/{Stats[CharacterStat.MaxTP]}";
        }

        protected override void DrawStatusBar()
        {
            //todo: figure out these magic numbers
            var srcWidth = 25 + (int)Math.Round(Stats[CharacterStat.TP] / (double)Stats[CharacterStat.MaxTP] * 79);
            var maskSrc = new Rectangle(_sourceRectangleArea.X, _sourceRectangleArea.Height, srcWidth, _sourceRectangleArea.Height);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_texture, DrawPositionWithParentOffset, _sourceRectangleArea, Color.White);
            _spriteBatch.Draw(_texture, DrawPositionWithParentOffset, maskSrc, Color.White);
            _spriteBatch.End();
        }
    }
}
