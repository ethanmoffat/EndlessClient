using System;
using EndlessClient.Input;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.StatusBars
{
    public class SPStatusBar : StatusBarBase
    {
        public SPStatusBar(INativeGraphicsManager nativeGraphicsManager,
                           ICharacterProvider characterProvider)
            : base(nativeGraphicsManager, characterProvider)
        {
            DrawArea = new Rectangle(320, 0, _sourceRectangleArea.Width, _sourceRectangleArea.Height);
            _sourceRectangleArea.Offset(_sourceRectangleArea.Width * 2, 0);
        }

        protected override void UpdateLabelText()
        {
            _label.Text = $"{Stats[CharacterStat.SP]}/{Stats[CharacterStat.MaxSP]}";
        }

        protected override void DrawStatusBar()
        {
            //todo: figure out these magic numbers
            var srcWidth = 25 + (int)Math.Round(Stats[CharacterStat.SP] / (double)Stats[CharacterStat.MaxSP] * 79);
            var maskSrc = new Rectangle(_sourceRectangleArea.X, _sourceRectangleArea.Height, srcWidth, _sourceRectangleArea.Height);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_texture, DrawPositionWithParentOffset, _sourceRectangleArea, Color.White);
            _spriteBatch.Draw(_texture, DrawPositionWithParentOffset, maskSrc, Color.White);
            _spriteBatch.End();
        }
    }
}
