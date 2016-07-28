// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Character;

namespace EOLib.Domain.Extensions
{
    public static class CharacterExtensions
    {
        public static int GetDestinationX(this ICharacter character)
        {
            if (character.RenderProperties.CurrentAction != CharacterActionState.Walking)
                throw new ArgumentException("The character is not currently in the walking state.", "character");

            var offset = GetXOffset(character.RenderProperties.Direction);
            return character.MapX + offset;
        }

        public static int GetDestinationY(this ICharacter character)
        {
            if (character.RenderProperties.CurrentAction != CharacterActionState.Walking)
                throw new ArgumentException("The character is not currently in the walking state.", "character");

            var offset = GetYOffset(character.RenderProperties.Direction);
            return character.MapY + offset;
        }

        private static int GetXOffset(EODirection direction)
        {
            return direction == EODirection.Right ? 1 :
                   direction == EODirection.Left ? -1 : 0;
        }

        private static int GetYOffset(EODirection direction)
        {
            return direction == EODirection.Down ? 1 :
                   direction == EODirection.Up ? -1 : 0;
        }
    }
}
