using AutomaticTypeMapper;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;

namespace EndlessClient.Rendering
{
    [MappedType(BaseType = typeof(IRenderOffsetCalculator))]
    public class RenderOffsetCalculator : IRenderOffsetCalculator
    {
        private const int WidthFactor = 32;
        private const int HeightFactor = 16;
        private const int WalkWidthFactor = WidthFactor / 4;
        private const int WalkHeightFactor = HeightFactor / 4;

        public int CalculateOffsetX(CharacterRenderProperties properties)
        {
            return properties.MapX * WidthFactor - properties.MapY * WidthFactor + CalculateWalkAdjustX(properties);
        }

        public int CalculateWalkAdjustX(CharacterRenderProperties properties)
        {
            var multiplier = properties.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;
            var walkAdjust = properties.IsActing(CharacterActionState.Walking) ? WalkWidthFactor * properties.ActualWalkFrame : 0;
            return walkAdjust * multiplier;
        }

        public int CalculateOffsetY(CharacterRenderProperties properties)
        {
            return properties.MapX * HeightFactor + properties.MapY * HeightFactor + CalculateWalkAdjustY(properties);
        }

        public int CalculateWalkAdjustY(CharacterRenderProperties properties)
        {
            var multiplier = properties.IsFacing(EODirection.Left, EODirection.Up) ? -1 : 1;
            var walkAdjust = properties.IsActing(CharacterActionState.Walking) ? WalkHeightFactor * properties.ActualWalkFrame : 0;
            return walkAdjust * multiplier;
        }

        public int CalculateOffsetX(EOLib.Domain.NPC.NPC npc)
        {
            return npc.X * WidthFactor - npc.Y * WidthFactor + CalculateWalkAdjustX(npc);
        }

        public int CalculateWalkAdjustX(EOLib.Domain.NPC.NPC npc)
        {
            var multiplier = npc.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;
            var walkAdjust = npc.IsActing(NPCActionState.Walking) ? WalkWidthFactor * npc.GetWalkFrame() : 0;

            return walkAdjust * multiplier;
        }

        public int CalculateOffsetY(EOLib.Domain.NPC.NPC npc)
        {
            return npc.X * HeightFactor + npc.Y * HeightFactor + CalculateWalkAdjustY(npc);
        }

        public int CalculateWalkAdjustY(EOLib.Domain.NPC.NPC npc)
        {
            var multiplier = npc.IsFacing(EODirection.Left, EODirection.Up) ? -1 : 1;
            var walkAdjust = npc.IsActing(NPCActionState.Walking) ? WalkHeightFactor * npc.GetWalkFrame() : 0;

            return walkAdjust * multiplier;
        }

        public int CalculateOffsetX(MapCoordinate coordinate)
        {
            return coordinate.X * WidthFactor - coordinate.Y * WidthFactor;
        }

        public int CalculateOffsetY(MapCoordinate coordinate)
        {
            return coordinate.X * HeightFactor + coordinate.Y * HeightFactor;
        }
    }

    public interface IRenderOffsetCalculator
    {
        int CalculateOffsetX(CharacterRenderProperties properties);
        int CalculateWalkAdjustX(CharacterRenderProperties properties);

        int CalculateOffsetY(CharacterRenderProperties properties);
        int CalculateWalkAdjustY(CharacterRenderProperties properties);

        int CalculateOffsetX(EOLib.Domain.NPC.NPC npc);
        int CalculateWalkAdjustX(EOLib.Domain.NPC.NPC npc);

        int CalculateOffsetY(EOLib.Domain.NPC.NPC npc);
        int CalculateWalkAdjustY(EOLib.Domain.NPC.NPC npc);

        int CalculateOffsetX(MapCoordinate coordinate);

        int CalculateOffsetY(MapCoordinate coordinate);
    }
}
