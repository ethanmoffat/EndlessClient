using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using System;

using DomainNPC = EOLib.Domain.NPC.NPC;

namespace EndlessClient.Rendering
{
    [AutoMappedType]
    public class GridDrawCoordinateCalculator : IGridDrawCoordinateCalculator
    {
        private const int GridSpaceWidth = 64;
        private const int GridSpaceHeight = 32;

        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;

        public GridDrawCoordinateCalculator(ICharacterProvider characterProvider,
                                            ICurrentMapProvider currentMapProvider,
                                            IRenderOffsetCalculator renderOffsetCalculator,
                                            IClientWindowSizeProvider clientWindowSizeProvider)
        {
            _characterProvider = characterProvider;
            _currentMapProvider = currentMapProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
            _clientWindowSizeProvider = clientWindowSizeProvider;
        }

        public Vector2 CalculateDrawCoordinatesFromGridUnits(int gridX, int gridY)
        {
            var widthFactor = _clientWindowSizeProvider.Width / 2; // 640 * (1/2) - 1
            var heightFactor = _clientWindowSizeProvider.Resizable
                ? _clientWindowSizeProvider.Height / 2
                : _clientWindowSizeProvider.Height * 3 / 10 - 2; // 480 * (3/10) - 2

            return new Vector2(widthFactor + (gridX * 32) - (gridY * 32),
                               heightFactor + (gridY * 16) + (gridX * 16)) - GetMainCharacterOffsets();
        }

        public Vector2 CalculateDrawCoordinatesFromGridUnits(MapCoordinate mapCoordinate)
        {
            return CalculateDrawCoordinatesFromGridUnits(mapCoordinate.X, mapCoordinate.Y);
        }

        public Vector2 CalculateBaseLayerDrawCoordinatesFromGridUnits(int gridX, int gridY)
        {
            var widthFactor = (_clientWindowSizeProvider.Width - GridSpaceWidth) / 2; // 288 = (640 - 64) / 2
            var heightFactor = _clientWindowSizeProvider.Resizable
                ? _clientWindowSizeProvider.Height / 2
                : _clientWindowSizeProvider.Height * 3 / 10 - 2; // 144 = 480 * (3/10)

            return new Vector2(widthFactor + (gridX * 32) - (gridY * 32),
                               heightFactor + (gridY * 16) + (gridX * 16)) - GetMainCharacterOffsets();
        }

        public Vector2 CalculateBaseLayerDrawCoordinatesFromGridUnits(MapCoordinate mapCoordinate)
        {
            return CalculateBaseLayerDrawCoordinatesFromGridUnits(mapCoordinate.X, mapCoordinate.Y);
        }

        public Vector2 CalculateGroundLayerDrawCoordinatesFromGridUnits()
        {
            var ViewportWidthFactor = _clientWindowSizeProvider.Width / 2 - 1; // 640 * (1/2) - 1
            var ViewportHeightFactor = _clientWindowSizeProvider.Resizable
                ? _clientWindowSizeProvider.Height / 2
                : _clientWindowSizeProvider.Height * 3 / 10 - 2; // 480 * (3/10) - 2

            var props = _characterProvider.MainCharacter.RenderProperties;

            var mapHeightPlusOne = _currentMapProvider.CurrentMap.Properties.Height + 1;

            // opposite of the algorithm for rendering the base layers
            return new Vector2(ViewportWidthFactor - (mapHeightPlusOne * 32) + (props.MapY * 32) - (props.MapX * 32),
                               ViewportHeightFactor - (props.MapY * 16) - (props.MapX * 16)) - GetMainCharacterWalkAdjustOffsets();
        }

        public Vector2 CalculateDrawCoordinates(DomainNPC npc)
        {
            var ViewportWidthFactor = _clientWindowSizeProvider.Width / 2 - 1; // 640 * (1/2) - 1
            var ViewportHeightFactor = _clientWindowSizeProvider.Resizable
                ? _clientWindowSizeProvider.Height / 2
                : _clientWindowSizeProvider.Height * 3 / 10 - 2; // 480 * (3/10) - 1 // ???

            var npcOffsetX = _renderOffsetCalculator.CalculateOffsetX(npc);
            var npcOffsetY = _renderOffsetCalculator.CalculateOffsetY(npc);

            var mainOffsetX = _renderOffsetCalculator.CalculateOffsetX(_characterProvider.MainCharacter.RenderProperties);
            var mainOffsetY = _renderOffsetCalculator.CalculateOffsetY(_characterProvider.MainCharacter.RenderProperties);

            return new Vector2(ViewportWidthFactor + npcOffsetX - mainOffsetX,
                               ViewportHeightFactor + npcOffsetY - mainOffsetY + 16);
        }

        public MapCoordinate CalculateGridCoordinatesFromDrawLocation(Vector2 drawLocation)
        {
            //need to solve this system of equations to get x, y on the grid
            //(x * 32) - (y * 32) + 288 - c.OffsetX, => pixX = 32x - 32y + 288 - c.OffsetX
            //(y * 16) + (x * 16) + 144 - c.OffsetY  => 2pixY = 32y + 32x + 288 - 2c.OffsetY
            //                                         => 2pixY + pixX = 64x + 576 - c.OffsetX - 2c.OffsetY
            //                                         => 2pixY + pixX - 576 + c.OffsetX + 2c.OffsetY = 64x
            //                                         => _gridX = (pixX + 2pixY - 576 + c.OffsetX + 2c.OffsetY) / 64; <=
            //pixY = (_gridX * 16) + (_gridY * 16) + 144 - c.OffsetY =>
            //(pixY - (_gridX * 16) - 144 + c.OffsetY) / 16 = _gridY

            if (_clientWindowSizeProvider.Resizable)
            {
                var msX = drawLocation.X;
                var msY = drawLocation.Y - GridSpaceHeight / 2;

                var widthFactor = _clientWindowSizeProvider.Resizable
                    ? _clientWindowSizeProvider.Width / 2 // 288 = 640 * .45, viewport width factor
                    : _clientWindowSizeProvider.Width * 9 / 10; // 288 = 640 * .45, 576 = 640 * .9
                var heightFactor = _clientWindowSizeProvider.Resizable
                    ? _clientWindowSizeProvider.Height / 2 // 144 = 480 * .45, viewport height factor
                    : _clientWindowSizeProvider.Height * 3 / 10;

                var offsetX = _renderOffsetCalculator.CalculateOffsetX(_characterProvider.MainCharacter.RenderProperties);
                var offsetY = _renderOffsetCalculator.CalculateOffsetY(_characterProvider.MainCharacter.RenderProperties);

                var gridX = (int)Math.Round((msX + (2 * msY) - widthFactor - heightFactor * 2 + offsetX + (2 * offsetY)) / 64.0);
                var gridY = (int)Math.Round((msY - (16 * gridX) - heightFactor + offsetY) / 16.0);

                return new MapCoordinate(gridX, gridY);
            }
            else
            {
                const int ViewportWidthFactor = 288; // 640 * (1/2) - 32
                const int ViewportHeightFactor = 142; // 480 * (3/10) - 2

                var msX = drawLocation.X - GridSpaceWidth / 2;
                var msY = drawLocation.Y - GridSpaceHeight / 2;

                var widthFactor = _clientWindowSizeProvider.Width * 9 / 10; // 288 = 640 * .45, 576 = 640 * .9
                var heightFactor = _clientWindowSizeProvider.Height * 3 / 10;

                var offsetX = _renderOffsetCalculator.CalculateOffsetX(_characterProvider.MainCharacter.RenderProperties);
                var offsetY = _renderOffsetCalculator.CalculateOffsetY(_characterProvider.MainCharacter.RenderProperties);

                var gridX = (int)Math.Round((msX + 2 * msY - (ViewportWidthFactor*2) + offsetX + 2 * offsetY) / 64.0);
                var gridY = (int)Math.Round((msY - gridX * 16 - ViewportHeightFactor + offsetY) / 16.0);

                return new MapCoordinate(gridX, gridY);
            }
        }

        private Vector2 GetMainCharacterOffsets()
        {
            var props = _characterProvider.MainCharacter.RenderProperties;
            return new Vector2(_renderOffsetCalculator.CalculateOffsetX(props),
                               _renderOffsetCalculator.CalculateOffsetY(props));
        }

        private Vector2 GetMainCharacterWalkAdjustOffsets()
        {
            var props = _characterProvider.MainCharacter.RenderProperties;
            return new Vector2(_renderOffsetCalculator.CalculateWalkAdjustX(props),
                               _renderOffsetCalculator.CalculateWalkAdjustY(props));
        }
    }

    public interface IGridDrawCoordinateCalculator
    {
        Vector2 CalculateDrawCoordinatesFromGridUnits(int gridX, int gridY);

        Vector2 CalculateDrawCoordinatesFromGridUnits(MapCoordinate mapCoordinate);

        Vector2 CalculateBaseLayerDrawCoordinatesFromGridUnits(int gridX, int gridY);

        Vector2 CalculateBaseLayerDrawCoordinatesFromGridUnits(MapCoordinate mapCoordinate);

        Vector2 CalculateGroundLayerDrawCoordinatesFromGridUnits();

        Vector2 CalculateDrawCoordinates(DomainNPC npc);

        MapCoordinate CalculateGridCoordinatesFromDrawLocation(Vector2 drawLocation);
    }
}
