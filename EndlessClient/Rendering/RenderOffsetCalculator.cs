// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.NPC;

namespace EndlessClient.Rendering
{
    public class RenderOffsetCalculator : IRenderOffsetCalculator
    {
        private const int WidthFactor = 32;
        private const int HeightFactor = 16;
        private const int WalkWidthFactor = WidthFactor/4;
        private const int WalkHeightFactor = HeightFactor/4;

        public int CalculateOffsetX(ICharacterRenderProperties properties)
        {
            var multiplier = properties.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;
            var walkAdjust = properties.IsActing(CharacterActionState.Walking) ? WalkWidthFactor * properties.WalkFrame : 0;

            //walkAdjust * multiplier is the old ViewAdjustX
            return properties.MapX*WidthFactor - properties.MapY*WidthFactor + walkAdjust*multiplier;
        }

        public int CalculateOffsetY(ICharacterRenderProperties properties)
        {
            var multiplier = properties.IsFacing(EODirection.Left, EODirection.Up) ? -1 : 1;
            var walkAdjust = properties.IsActing(CharacterActionState.Walking) ? WalkHeightFactor * properties.WalkFrame : 0;

            //walkAdjust * multiplier is the old ViewAdjustY
            return properties.MapX*HeightFactor + properties.MapY*HeightFactor + walkAdjust*multiplier;
        }

        public int CalculateOffsetX(INPC npc)
        {
            var multiplier = npc.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;
            var walkAdjust = npc.IsActing(NPCActionState.Walking) ? WalkWidthFactor * npc.GetWalkFrame() : 0;

            //walkAdjust * multiplier is the old ViewAdjustX
            return npc.X*WidthFactor - npc.Y*WidthFactor + walkAdjust*multiplier;
        }

        public int CalculateOffsetY(INPC npc)
        {
            var multiplier = npc.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;
            var walkAdjust = npc.IsActing(NPCActionState.Walking) ? WalkHeightFactor * npc.GetWalkFrame() : 0;

            //walkAdjust * multiplier is the old ViewAdjustY
            return npc.X*HeightFactor - npc.Y*HeightFactor + walkAdjust*multiplier;
        }
    }

    public interface IRenderOffsetCalculator
    {
        int CalculateOffsetX(ICharacterRenderProperties properties);

        int CalculateOffsetY(ICharacterRenderProperties properties);

        int CalculateOffsetX(INPC npc);

        int CalculateOffsetY(INPC npc);
    }
}