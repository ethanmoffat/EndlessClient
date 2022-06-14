using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using XNAControls;

namespace EndlessClient.Rendering
{
    public class MiniMapRenderer : XNAControl
    {
        private enum MiniMapGfx
        {
            UpLine = 0,
            LeftLine = 1,
            Corner = 2,
            Solid = 3, //wall or obstacle
            Green = 4, //other player
            Red = 5, //attackable npc
            Orange = 6, //you!
            Blue = 7, //tile that you can interact with
            Purple = 8, //npc
            NUM_GRIDS = 9,
        }

        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly Texture2D _miniMapTexture;

        public MiniMapRenderer(INativeGraphicsManager nativeGraphicsManager,
                               ICurrentMapProvider currentMapProvider,
                               ICurrentMapStateProvider currentMapStateProvider,
                               ICharacterProvider characterProvider,
                               IMapCellStateProvider mapCellStateProvider,
                               IENFFileProvider enfFileProvider)
        {
            _currentMapProvider = currentMapProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _characterProvider = characterProvider;
            _mapCellStateProvider = mapCellStateProvider;
            _enfFileProvider = enfFileProvider;
            _miniMapTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 45, true);
        }

        public override void Initialize()
        {
            Visible = true;
            DrawOrder = 0;

            base.Initialize();
        }

        protected override bool ShouldDraw()
        {
            return base.ShouldDraw() && _currentMapStateProvider.ShowMiniMap;
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();

            var (cx, cy) = GetCharacterPos();
            for (int row = Math.Max(cy - 30, 0); row <= Math.Min(cy + 30, _currentMapProvider.CurrentMap.Properties.Height); ++row)
            {
                for (int col = Math.Max(cx - 30, 0); col <= Math.Min(cx + 30, _currentMapProvider.CurrentMap.Properties.Width); ++col)
                {
                    var loc = GetMiniMapDrawCoordinates(col, row);
                    var (isEdge, miniMapRectSrc) = GetSourceRectangleForGridSpace(col, row);
                    DrawGridBox(loc, isEdge, miniMapRectSrc);
                }
            }

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        private (bool IsEdge, Rectangle SourceRect) GetSourceRectangleForGridSpace(int col, int row)
        {
            var (cx, cy) = GetCharacterPos();
            if (cx == col && cy == row)
            {
                return (false, GetSourceRect(MiniMapGfx.Orange));
            }

            var info = _mapCellStateProvider.GetCellStateAt(col, row);

            if (info.NPC.HasValue)
            {
                return info.NPC.Map(x => _enfFileProvider.ENFFile[x.ID].Type)
                    .Map(x => x == NPCType.Aggressive || x == NPCType.Passive ? MiniMapGfx.Red : MiniMapGfx.Purple)
                    .Match(x => (false, GetSourceRect(x)), () => (false, Rectangle.Empty));
            }
            else if (info.Character.HasValue)
            {
                return (false, GetSourceRect(MiniMapGfx.Green));
            }
            else if (info.Warp.HasValue)
            {
                return info.Warp.Map(x => x.DoorType)
                    .Match(x => (false, GetSourceRect(x != DoorSpec.NoDoor ? MiniMapGfx.Blue : MiniMapGfx.UpLine)), () => (false, Rectangle.Empty));
            }
            else
            {
                switch (info.TileSpec)
                {
                    case TileSpec.Wall:
                    case TileSpec.FakeWall:
                        return (false, GetSourceRect(MiniMapGfx.Solid));
                    case TileSpec.BankVault:
                    case TileSpec.ChairAll:
                    case TileSpec.ChairDown:
                    case TileSpec.ChairLeft:
                    case TileSpec.ChairRight:
                    case TileSpec.ChairUp:
                    case TileSpec.ChairDownRight:
                    case TileSpec.ChairUpLeft:
                    case TileSpec.Chest:
                        return (false, GetSourceRect(MiniMapGfx.Blue));
                    case TileSpec.MapEdge:
                        return (true, GetSourceRect(MiniMapGfx.UpLine));
                }
            }

            return (false, GetSourceRect(MiniMapGfx.UpLine));
        }

        private void DrawGridBox(Vector2 loc, bool isEdge, Rectangle gridSpaceSourceRect)
        {
            if (!isEdge)
            {
                // draw grid lines on each grid space if it isn't an edge space
                _spriteBatch.Draw(_miniMapTexture, loc,
                    new Rectangle((int)MiniMapGfx.UpLine * gridSpaceSourceRect.Width, 0, gridSpaceSourceRect.Width, gridSpaceSourceRect.Height),
                    Color.FromNonPremultiplied(255, 255, 255, 128));
                _spriteBatch.Draw(_miniMapTexture, loc,
                    new Rectangle((int)MiniMapGfx.LeftLine * gridSpaceSourceRect.Width, 0, gridSpaceSourceRect.Width, gridSpaceSourceRect.Height),
                    Color.FromNonPremultiplied(255, 255, 255, 128));
            }

            _spriteBatch.Draw(_miniMapTexture, loc, gridSpaceSourceRect, Color.FromNonPremultiplied(255, 255, 255, 128));
        }

        private Vector2 GetMiniMapDrawCoordinates(int x, int y)
        {
            // holy magic numbers batman
            // TODO - make this not gross
            var (cx, cy) = GetCharacterPos();
            return new Vector2((x * 13) - (y * 13) + 288 - (cx * 13 - cy * 13), (y * 7) + (x * 7) + 144 - (cx * 7 + cy * 7));
        }

        private Rectangle GetSourceRect(MiniMapGfx gfx)
        {
            var delta = _miniMapTexture.Width / (int)MiniMapGfx.NUM_GRIDS;
            return new Rectangle((int)gfx * delta, 0, delta, _miniMapTexture.Height);
        }

        private (int X, int Y) GetCharacterPos()
        {
            var rp = _characterProvider.MainCharacter.RenderProperties;
            if (rp.CurrentAction == CharacterActionState.Walking)
                return (rp.GetDestinationX(), rp.GetDestinationY());
            else
                return (rp.MapX, rp.MapY);
        }
    }
}
