using System;
using System.Collections.Generic;
using EndlessClient.Input;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.StatusBars
{
    public class TNLStatusBar : StatusBarBase
    {
        private readonly IExperienceTableProvider _experienceTableProvider;

        public TNLStatusBar(INativeGraphicsManager nativeGraphicsManager,
                            ICharacterProvider characterProvider,
                            IExperienceTableProvider experienceTableProvider)
            : base(nativeGraphicsManager, characterProvider)
        {
            _experienceTableProvider = experienceTableProvider;
            DrawArea = new Rectangle(430, 0, _sourceRectangleArea.Width, _sourceRectangleArea.Height);

            _sourceRectangleArea = new Rectangle(_sourceRectangleArea.Width*3 - 1,
                                                 0,
                                                 _sourceRectangleArea.Width + 1,
                                                 _sourceRectangleArea.Height);
        }

        protected override void UpdateLabelText()
        {
            _label.Text = $"{ExpTable[Stats[CharacterStat.Level] + 1] - Stats[CharacterStat.Experience]}";
        }

        protected override void DrawStatusBar()
        {
            //todo: figure out these magic numbers
            var thisLevelExp = ExpTable[Stats[CharacterStat.Level]];
            var nextLevelExp = ExpTable[Stats[CharacterStat.Level] + 1];
            var srcWidth = 25 + (int)Math.Round((Stats[CharacterStat.Experience] - thisLevelExp) / (double)(nextLevelExp - thisLevelExp) * 79);
            var maskSrc = new Rectangle(_sourceRectangleArea.X, _sourceRectangleArea.Height, srcWidth, _sourceRectangleArea.Height);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_texture, DrawPositionWithParentOffset, _sourceRectangleArea, Color.White);
            _spriteBatch.Draw(_texture, DrawPositionWithParentOffset, maskSrc, Color.White);
            _spriteBatch.End();
        }

        private IReadOnlyList<int> ExpTable => _experienceTableProvider.ExperienceByLevel;
    }
}
