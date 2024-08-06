using System.Collections.Generic;
using System.Linq;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Rendering.Map
{
    public class MiniMapRenderer : XNAControl
    {
        private const int TileWidth = 28;
        private const int TileHeight = 14;

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

        private readonly object _rt_locker_ = new object();
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;
        private readonly Texture2D _miniMapTexture;

        private RenderTarget2D _miniMapTarget;
        private IReadOnlyList<int> _lastMapChecksum;

        public MiniMapRenderer(INativeGraphicsManager nativeGraphicsManager,
                               IRenderTargetFactory renderTargetFactory,
                               IClientWindowSizeProvider clientWindowSizeProvider,
                               ICurrentMapProvider currentMapProvider,
                               ICurrentMapStateProvider currentMapStateProvider,
                               ICharacterProvider characterProvider,
                               IENFFileProvider enfFileProvider,
                               IGridDrawCoordinateCalculator gridDrawCoordinateCalculator)
        {
            _renderTargetFactory = renderTargetFactory;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _currentMapProvider = currentMapProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _characterProvider = characterProvider;
            _enfFileProvider = enfFileProvider;
            _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;
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
            return _currentMapStateProvider.ShowMiniMap && base.ShouldDraw();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_lastMapChecksum == null || !_lastMapChecksum.SequenceEqual(_currentMapProvider.CurrentMap.Properties.Checksum))
            {
                // The dimensions of the map are 0-based in the properties. Adjust to 1-based for RT creation
                var widthPlus1 = _currentMapProvider.CurrentMap.Properties.Width + 1;
                var heightPlus1 = _currentMapProvider.CurrentMap.Properties.Height + 1;

                lock (_rt_locker_)
                {
                    _miniMapTarget?.Dispose();
                    _miniMapTarget = _renderTargetFactory.CreateRenderTarget(
                        (widthPlus1 + heightPlus1) * TileWidth,
                        (widthPlus1 + heightPlus1) * TileHeight);
                }

                DrawFixedMapElementsToRenderTarget();
            }

            _lastMapChecksum = _currentMapProvider.CurrentMap.Properties.Checksum;

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            lock (_rt_locker_)
            {
                _spriteBatch.Begin();

                var baseTargetDrawLoc = _gridDrawCoordinateCalculator.CalculateGroundLayerRenderTargetDrawCoordinates(isMiniMap: true, TileWidth, TileHeight);
                _spriteBatch.Draw(_miniMapTarget, baseTargetDrawLoc, Color.White);

                var entities = new IMapEntity[] { _characterProvider.MainCharacter }
                    .Concat(_currentMapStateProvider.Characters)
                    .Concat(_currentMapStateProvider.NPCs);

                foreach (var entity in entities)
                {
                    var loc = GetMiniMapDrawCoordinates(entity.X, entity.Y);
                    var miniMapRectSrc = GetSourceRectangleForEntity(entity);
                    DrawGridBox(loc, null, miniMapRectSrc);
                }

                _spriteBatch.End();
            }

            base.OnDrawControl(gameTime);
        }

        private (MiniMapGfx? EdgeGfx, Rectangle SourceRect) GetSourceRectangleForGridSpace(int col, int row)
        {
            var tileSpec = _currentMapProvider.CurrentMap.Tiles[row, col];

            switch (tileSpec)
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

            if (_currentMapProvider.CurrentMap.Warps[row, col] != null)
            {
                var doorType = _currentMapProvider.CurrentMap.Warps[row, col].DoorType;
                return (GetEdge(), GetSourceRect(doorType != DoorSpec.NoDoor ? MiniMapGfx.Blue : MiniMapGfx.UpLine));
            }

            return (GetEdge(), Rectangle.Empty);

            MiniMapGfx? GetEdge()
            {
                if (tileSpec == TileSpec.MapEdge)
                    return null;

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

        private Rectangle GetSourceRectangleForEntity(IMapEntity mapEntity)
        {
            if (_characterProvider.MainCharacter == mapEntity)
            {
                return GetSourceRect(MiniMapGfx.Orange);
            }

            return mapEntity switch
            {
                EOLib.Domain.NPC.NPC n => GetNPCSourceRectangle(n),
                EOLib.Domain.Character.Character c => GetSourceRect(MiniMapGfx.Green),
                _ => Rectangle.Empty
            };

            Rectangle GetNPCSourceRectangle(EOLib.Domain.NPC.NPC npc)
            {
                var npcType = _enfFileProvider.ENFFile[npc.ID].Type;
                return GetSourceRect(npcType == NPCType.Aggressive || npcType == NPCType.Passive ? MiniMapGfx.Red : MiniMapGfx.Purple);
            }
        }

        private void DrawFixedMapElementsToRenderTarget()
        {
            if (_lastMapChecksum != null && _lastMapChecksum.SequenceEqual(_currentMapProvider.CurrentMap.Properties.Checksum))
                return;

            GraphicsDevice.SetRenderTarget(_miniMapTarget);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);
            _spriteBatch.Begin();

            // the height is used to offset the 0 point of the grid, which is TileHeight units per tile in the height of the map
            var height = _currentMapProvider.CurrentMap.Properties.Height + 1;

            for (int row = 0; row <= _currentMapProvider.CurrentMap.Properties.Height; ++row)
            {
                for (int col = 0; col <= _currentMapProvider.CurrentMap.Properties.Width; ++col)
                {
                    var drawLoc = _gridDrawCoordinateCalculator.CalculateRawRenderCoordinatesFromGridUnits(col, row, TileWidth, TileHeight) + new Vector2(TileHeight * height, 0);
                    var (edgeGfx, miniMapRectSrc) = GetSourceRectangleForGridSpace(col, row);
                    DrawGridBox(drawLoc, edgeGfx, miniMapRectSrc);
                }
            }

            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
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

            if (!gridSpaceSourceRect.IsEmpty)
            {
                _spriteBatch.Draw(_miniMapTexture, loc, gridSpaceSourceRect, Color.FromNonPremultiplied(255, 255, 255, 128));
            }
        }

        private Vector2 GetMiniMapDrawCoordinates(int x, int y)
        {
            var widthFactor = _clientWindowSizeProvider.Width / 2;
            var heightFactor = _clientWindowSizeProvider.Resizable
                ? _clientWindowSizeProvider.Height / 2 // 144 = 480 * .45, viewport height factor
                : _clientWindowSizeProvider.Height * 3 / 10 - 2;

            var tileWidthFactor = TileWidth / 2;
            var tileHeightFactor = TileHeight / 2;

            return new Vector2(x * tileWidthFactor - y * tileWidthFactor + widthFactor,
                               y * tileHeightFactor + x * tileHeightFactor + heightFactor) - GetCharacterOffset();
        }

        private Rectangle GetSourceRect(MiniMapGfx gfx)
        {
            var delta = _miniMapTexture.Width / (int)MiniMapGfx.NUM_GRIDS;
            return new Rectangle((int)gfx * delta, 0, delta, _miniMapTexture.Height);
        }

        private Vector2 GetCharacterOffset()
        {
            var tileWidthFactor = TileWidth / 2;
            var tileHeightFactor = TileHeight / 2;

            var (cx, cy) = (_characterProvider.MainCharacter.X, _characterProvider.MainCharacter.Y);
            return new Vector2(cx * tileWidthFactor - cy * tileWidthFactor, cx * tileHeightFactor + cy * tileHeightFactor);
        }
    }
}
