// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class CharacterRenderOffsetCalculator : ICharacterRenderOffsetCalculator
    {
        public int CalculateOffsetX(ICharacter character)
        {
            var properties = character.RenderProperties;

            var multiplier = properties.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;
            var walkAdjust = properties.IsActing(CharacterActionState.Walking) ? 8 * properties.WalkFrame : 0;

            //walkAdjust * multiplier is the old ViewAdjustX
            return character.MapX*32 - character.MapY*32 + walkAdjust * multiplier;
        }

        public int CalculateOffsetY(ICharacter character)
        {
            var properties = character.RenderProperties;

            var multiplier = properties.IsFacing(EODirection.Left, EODirection.Up) ? -1 : 1;
            var walkAdjust = properties.IsActing(CharacterActionState.Walking) ? 4 * properties.WalkFrame : 0;

            //walkAdjust * multiplier is the old ViewAdjustY
            return character.MapX*16 + character.MapY*16 + walkAdjust * multiplier;
        }
    }
}