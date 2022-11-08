using AutomaticTypeMapper;
using EndlessClient.Rendering.NPC;
using EOLib;
using EOLib.Domain.NPC;
using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Sprites
{
    [MappedType(BaseType = typeof(INPCSpriteSheet))]
    public class NPCSpriteSheet : INPCSpriteSheet
    {
        private readonly INativeGraphicsManager _gfxManager;

        public NPCSpriteSheet(INativeGraphicsManager gfxManager)
        {
            _gfxManager = gfxManager;
        }

        public Texture2D GetNPCTexture(int baseGraphic, NPCFrame whichFrame, EODirection direction)
        {
            int offset;
            switch (whichFrame)
            {
                case NPCFrame.Standing:
                    offset = direction == EODirection.Down || direction == EODirection.Right ? 1 : 3;
                    break;
                case NPCFrame.StandingFrame1:
                    offset = direction == EODirection.Down || direction == EODirection.Right ? 2 : 4;
                    break;
                case NPCFrame.WalkFrame1:
                    offset = direction == EODirection.Down || direction == EODirection.Right ? 5 : 9;
                    break;
                case NPCFrame.WalkFrame2:
                    offset = direction == EODirection.Down || direction == EODirection.Right ? 6 : 10;
                    break;
                case NPCFrame.WalkFrame3:
                    offset = direction == EODirection.Down || direction == EODirection.Right ? 7 : 11;
                    break;
                case NPCFrame.WalkFrame4:
                    offset = direction == EODirection.Down || direction == EODirection.Right ? 8 : 12;
                    break;
                case NPCFrame.Attack1:
                    offset = direction == EODirection.Down || direction == EODirection.Right ? 13 : 15;
                    break;
                case NPCFrame.Attack2:
                    offset = direction == EODirection.Down || direction == EODirection.Right ? 14 : 16;
                    break;
                default:
                    return null;
            }

            var baseGfx = (baseGraphic - 1) * 40;
            return _gfxManager.TextureFromResource(GFXTypes.NPC, baseGfx + offset, true);
        }

        public NPCFrameMetadata GetNPCMetadata(int graphic)
        {
            // todo: load from GFX file RCData
            switch (graphic)
            {
                case 1: // crow
                    return new NPCFrameMetadata.Builder
                    {
                        OffsetX = 0,
                        OffsetY = 16,
                        AttackOffsetX = 0,
                        AttackOffsetY = 0,
                        HasStandingFrameAnimation = false,
                        NameLabelOffset = 4,
                    }.ToImmutable();
                case 2: // rat
                    return new NPCFrameMetadata.Builder
                    {
                        OffsetX = 0,
                        OffsetY = 18,
                        AttackOffsetX = -6,
                        AttackOffsetY = -3,
                        HasStandingFrameAnimation = false,
                        NameLabelOffset = 0,
                    }.ToImmutable();
                case 16: // doot doot bois
                case 17:
                case 18:
                    return new NPCFrameMetadata.Builder
                    {
                        OffsetX = 0,
                        OffsetY = 14,
                        AttackOffsetX = -6,
                        AttackOffsetY = -3,
                        HasStandingFrameAnimation = false,
                        NameLabelOffset = 45,
                    }.ToImmutable();
                case 107: // apozen
                    return new NPCFrameMetadata.Builder
                    {
                        OffsetX = -31,
                        OffsetY = 4,
                        AttackOffsetX = -4,
                        AttackOffsetY = -2,
                        HasStandingFrameAnimation = false,
                        NameLabelOffset = 138,
                    }.ToImmutable();
                case 113: // robo tile
                    return new NPCFrameMetadata.Builder
                    {
                        OffsetX = 1,
                        OffsetY = 7,
                        AttackOffsetX = 0,
                        AttackOffsetY = 0,
                        HasStandingFrameAnimation = false,
                        NameLabelOffset = 6
                    }.ToImmutable();
                default: return new NPCFrameMetadata.Builder().ToImmutable();
            }
        }
    }

    public interface INPCSpriteSheet
    {
        Texture2D GetNPCTexture(int baseGraphic, NPCFrame whichFrame, EODirection direction);

        NPCFrameMetadata GetNPCMetadata(int graphic);
    }
}