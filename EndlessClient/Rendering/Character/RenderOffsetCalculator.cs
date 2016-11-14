// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;

namespace EndlessClient.Rendering.Character
{
    public class RenderOffsetCalculator : IRenderOffsetCalculator
    {
        public int CalculateOffsetX(ICharacterRenderProperties properties)
        {
            var multiplier = properties.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;
            var walkAdjust = properties.IsActing(CharacterActionState.Walking) ? 8 * properties.WalkFrame : 0;

            //walkAdjust * multiplier is the old ViewAdjustX
            return properties.MapX*32 - properties.MapY*32 + walkAdjust*multiplier;
        }

        public int CalculateOffsetY(ICharacterRenderProperties properties)
        {
            var multiplier = properties.IsFacing(EODirection.Left, EODirection.Up) ? -1 : 1;
            var walkAdjust = properties.IsActing(CharacterActionState.Walking) ? 4 * properties.WalkFrame : 0;

            //walkAdjust * multiplier is the old ViewAdjustY
            return properties.MapX*16 + properties.MapY*16 + walkAdjust*multiplier;
        }
    }
}