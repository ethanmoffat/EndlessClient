using AutomaticTypeMapper;
using EndlessClient.Input;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using System.Windows.Controls;
using System;

namespace EndlessClient.Rendering
{
    [AutoMappedType]
    public class GridDrawCoordinateCalculator : IGridDrawCoordinateCalculator
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;

        public GridDrawCoordinateCalculator(ICharacterProvider characterProvider,
                                            ICurrentMapProvider currentMapProvider,
                                            IRenderOffsetCalculator renderOffsetCalculator)
        {
            _characterProvider = characterProvider;
            _currentMapProvider = currentMapProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
        }

        public Vector2 CalculateDrawCoordinatesFromGridUnits(int gridX, int gridY)
        {
            const int ViewportWidthFactor = 320; // 640 * (1/2)
            const int ViewportHeightFactor = 144; // 480 * (3/10)

            return new Vector2(ViewportWidthFactor + (gridX * 32) - (gridY * 32),
                               ViewportHeightFactor + (gridY * 16) + (gridX * 16)) - CalculateCharacterOffsets();
        }

        public Vector2 CalculateDrawCoordinatesFromGridUnits(MapCoordinate mapCoordinate)
        {
            return CalculateDrawCoordinatesFromGridUnits(mapCoordinate.X, mapCoordinate.Y);
        }

        public Vector2 CalculateBaseLayerDrawCoordinatesFromGridUnits(int gridX, int gridY)
        {
            const int ViewportWidthFactor = 288; // ???
            const int ViewportHeightFactor = 144; // 480 * (3/10)

            return new Vector2(ViewportWidthFactor + (gridX * 32) - (gridY * 32),
                               ViewportHeightFactor + (gridY * 16) + (gridX * 16)) - CalculateBaseLayerOffsets();
        }

        public Vector2 CalculateBaseLayerDrawCoordinatesFromGridUnits(MapCoordinate mapCoordinate)
        {
            return CalculateBaseLayerDrawCoordinatesFromGridUnits(mapCoordinate.X, mapCoordinate.Y);
        }

        public Vector2 CalculateGroundLayerDrawCoordinatesFromGridUnits()
        {
            const int ViewportWidthFactor = 320; // 640 * (1/2)
            const int ViewportHeightFactor = 144; // 480 * (3/10)

            var props = _characterProvider.MainCharacter.RenderProperties;

            var mapHeightPlusOne = _currentMapProvider.CurrentMap.Properties.Height + 1;

            // opposite of the algorithm for rendering the base layers
            return new Vector2(ViewportWidthFactor - (mapHeightPlusOne * 32) + (props.MapY * 32) - (props.MapX * 32),
                               ViewportHeightFactor - (props.MapY * 16) - (props.MapX * 16)) - CalculateGroundLayerCharacterOffsets();
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

            const int GridSpaceWidth = 64;
            const int GridSpaceHeight = 32;

            var msX = drawLocation.X - GridSpaceWidth / 2;
            var msY = drawLocation.Y - GridSpaceHeight / 2;

            var offsetX = _renderOffsetCalculator.CalculateOffsetX(_characterProvider.MainCharacter.RenderProperties);
            var offsetY = _renderOffsetCalculator.CalculateOffsetY(_characterProvider.MainCharacter.RenderProperties);

            var gridX = (int)Math.Round((msX + 2 * msY - 576 + offsetX + 2 * offsetY) / 64.0);
            var gridY = (int)Math.Round((msY - gridX * 16 - 144 + offsetY) / 16.0);

            return new MapCoordinate(gridX, gridY);
        }

        private Vector2 CalculateCharacterOffsets()
        {
            var props = _characterProvider.MainCharacter.RenderProperties;
            return new Vector2(_renderOffsetCalculator.CalculateOffsetX(props),
                               _renderOffsetCalculator.CalculateOffsetY(props));
        }

        private Vector2 CalculateBaseLayerOffsets()
        {
            var props = _characterProvider.MainCharacter.RenderProperties;
            props = props.IsActing(CharacterActionState.Walking)
                ? props.WithActualWalkFrame(props.ActualWalkFrame - 1)
                : props;

            return new Vector2(_renderOffsetCalculator.CalculateOffsetX(props),
                               _renderOffsetCalculator.CalculateOffsetY(props));
        }

        private Vector2 CalculateGroundLayerCharacterOffsets()
        {
            // todo: WTF
            // this fixes the weird shifting issue with the base layers
            // not sure why they're weirdly offset in the first place
            var props = _characterProvider.MainCharacter.RenderProperties;
            props = props.IsActing(CharacterActionState.Walking)
                ? props.WithActualWalkFrame(props.ActualWalkFrame - 1)
                : props;

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

        MapCoordinate CalculateGridCoordinatesFromDrawLocation(Vector2 drawLocation);
    }
}
