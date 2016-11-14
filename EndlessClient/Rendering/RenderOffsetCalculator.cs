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

        public int CalculateOffsetX(INPC npc)
        {
            var multiplier = npc.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;
            var walkAdjust = npc.IsActing(NPCActionState.Walking) ? 8 * npc.GetWalkFrame() : 0;

            //walkAdjust * multiplier is the old ViewAdjustX
            return npc.X*32 - npc.Y*32 + walkAdjust*multiplier;
        }

        public int CalculateOffsetY(INPC npc)
        {
            var multiplier = npc.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;
            var walkAdjust = npc.IsActing(NPCActionState.Walking) ? 4 * npc.GetWalkFrame() : 0;

            //walkAdjust * multiplier is the old ViewAdjustY
            return npc.X*16 - npc.Y*16 + walkAdjust*multiplier;
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