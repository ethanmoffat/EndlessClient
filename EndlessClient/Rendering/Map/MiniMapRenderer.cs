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

namespace EndlessClient.Rendering.Map
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

        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly Texture2D _miniMapTexture;

        public MiniMapRenderer(INativeGraphicsManager nativeGraphicsManager,
                               IClientWindowSizeProvider clientWindowSizeProvider,
                               ICurrentMapProvider currentMapProvider,
                               ICurrentMapStateProvider currentMapStateProvider,
                               ICharacterProvider characterProvider,
                               IMapCellStateProvider mapCellStateProvider,
                               IENFFileProvider enfFileProvider)
        {
            _clientWindowSizeProvider = clientWindowSizeProvider;
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
                    var (edgeGfx, miniMapRectSrc) = GetSourceRectangleForGridSpace(col, row);
                    DrawGridBox(loc, edgeGfx, miniMapRectSrc);
                }
            }

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        private (MiniMapGfx? EdgeGfx, Rectangle SourceRect) GetSourceRectangleForGridSpace(int col, int row)
        {
            var info = _mapCellStateProvider.GetCellStateAt(col, row);

            var (cx, cy) = GetCharacterPos();
            if (cx == col && cy == row)
            {
                return (GetEdge(), GetSourceRect(MiniMapGfx.Orange));
            }

            if (info.NPC.HasValue)
            {
                return info.NPC.Map(x => _enfFileProvider.ENFFile[x.ID].Type)
                    .Map(x => x == NPCType.Aggressive || x == NPCType.Passive ? MiniMapGfx.Red : MiniMapGfx.Purple)
                    .Match(x => (GetEdge(), GetSourceRect(x)), () => (GetEdge(), Rectangle.Empty));
            }
            else if (info.Character.HasValue)
            {
                return (GetEdge(), GetSourceRect(MiniMapGfx.Green));
            }
            else if (info.Warp.HasValue)
            {
                return info.Warp.Map(x => x.DoorType)
                    .Match(x => (GetEdge(), GetSourceRect(x != DoorSpec.NoDoor ? MiniMapGfx.Blue : MiniMapGfx.UpLine)), () => (GetEdge(), Rectangle.Empty));
            }
            else
            {
                switch (info.TileSpec)
                {
                    case TileSpec.Wall:
                    case TileSpec.FakeWall:
                        return (GetEdge(), GetSourceRect(MiniMapGfx.Solid));
                    case TileSpec.BankVault:
                    case TileSpec.ChairAll:
                    case TileSpec.ChairDown:
                    case TileSpec.ChairLeft:
                    case TileSpec.ChairRight:
                    case TileSpec.ChairUp:
                    case TileSpec.ChairDownRight:
                    case TileSpec.ChairUpLeft:
                    case TileSpec.Chest:
                    case TileSpec.JammedDoor:
                        // Unknown TileSpecs 10-15 have been confirmed in the vanilla client to show a Blue ! on the minimap
                    case (TileSpec)10:
                    case (TileSpec)11:
                    case (TileSpec)12:
                    case (TileSpec)13:
                    case (TileSpec)14:
                    case (TileSpec)15:
                        return (GetEdge(), GetSourceRect(MiniMapGfx.Blue));
                    case TileSpec.MapEdge:
                        return (null, Rectangle.Empty);
                }
            }

            return (GetEdge(), Rectangle.Empty);

            MiniMapGfx? GetEdge()
            {
                if (info.TileSpec == TileSpec.MapEdge)
                    return null;

                var w = _currentMapProvider.CurrentMap.Properties.Width;
                var h = _currentMapProvider.CurrentMap.Properties.Height;
                var tiles = _currentMapProvider.CurrentMap.Tiles;

                if (col - 1 >= 0 && tiles[row, col - 1] == TileSpec.MapEdge &&
                    row - 1 >= 0 && tiles[row - 1, col] == TileSpec.MapEdge)
                    return null;
                else if (col == 0 || (col - 1 >= 0 && tiles[row, col - 1] == TileSpec.MapEdge))
                    return MiniMapGfx.UpLine;
                else if (row == 0 || (row - 1 >= 0 && tiles[row - 1, col] == TileSpec.MapEdge))
                    return MiniMapGfx.LeftLine;
                else
                    return MiniMapGfx.Corner;
            }
        }

        private void DrawGridBox(Vector2 loc, MiniMapGfx? edgeGfx, Rectangle gridSpaceSourceRect)
        {
            if (edgeGfx != null)
            {
                var src = gridSpaceSourceRect.Equals(Rectangle.Empty)
                    ? GetSourceRect(MiniMapGfx.UpLine)
                    : gridSpaceSourceRect;

                _spriteBatch.Draw(_miniMapTexture, loc,
                    new Rectangle((int)edgeGfx * src.Width, 0, src.Width, src.Height),
                    Color.FromNonPremultiplied(255, 255, 255, 128));
            }

            _spriteBatch.Draw(_miniMapTexture, loc, gridSpaceSourceRect, Color.FromNonPremultiplied(255, 255, 255, 128));
        }

        private Vector2 GetMiniMapDrawCoordinates(int x, int y)
        {
            // these are the same as in MouseCursorRenderer
            var widthFactor = _clientWindowSizeProvider.Resizable
                ? _clientWindowSizeProvider.Width / 2 // 288 = 640 * .45, viewport width factor
                : _clientWindowSizeProvider.Width * 9 / 10; // 288 = 640 * .45, 576 = 640 * .9
            var heightFactor = _clientWindowSizeProvider.Resizable
                ? _clientWindowSizeProvider.Height / 2 // 144 = 480 * .45, viewport height factor
                : _clientWindowSizeProvider.Height * 3 / 10;

            var tileWidthFactor = _miniMapTexture.Height; // 14
            var tileHeightFactor = _miniMapTexture.Height / 2; // 7

            var (cx, cy) = GetCharacterPos();
            return new Vector2(x * tileWidthFactor - y * tileWidthFactor + widthFactor - (cx * tileWidthFactor - cy * tileWidthFactor),
                               y * tileHeightFactor + x * tileHeightFactor + heightFactor - (cx * tileHeightFactor + cy * tileHeightFactor));
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
