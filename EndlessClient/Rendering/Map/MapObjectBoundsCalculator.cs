using AutomaticTypeMapper;
using EndlessClient.Rendering.MapEntityRenderers;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;

namespace EndlessClient.Rendering.Map
{
    [AutoMappedType]
    public class MapObjectBoundsCalculator : IMapObjectBoundsCalculator
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICharacterProvider _characterProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly MapObjectLayerRenderer _objectRenderer;

        public MapObjectBoundsCalculator(INativeGraphicsManager nativeGraphicsManager,
                                      ICharacterProvider characterProvider,
                                      IRenderOffsetCalculator renderOffsetCalculator,
                                      IMapEntityRendererProvider mapEntityRendererProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterProvider = characterProvider;
            _renderOffsetCalculator = renderOffsetCalculator;

            _objectRenderer = mapEntityRendererProvider.MapEntityRenderers.OfType<MapObjectLayerRenderer>().Single();
        }

        public Rectangle GetMapObjectBounds(int gridX, int gridY, int gfxNum)
        {
            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapObjects, gfxNum, transparent: true);
            var drawPosition = _objectRenderer.GetDrawCoordinatesFromGridUnits(gridX, gridY);
            // see: MapObjectLayerRenderer
            // todo: more centralized way of representing this
            drawPosition -= new Vector2(gfx.Width / 2, gfx.Height - 32);
            return gfx.Bounds.WithPosition(drawPosition);
        }
    }

    public interface IMapObjectBoundsCalculator
    {
        Rectangle GetMapObjectBounds(int gridX, int gridY, int gfxNum);
    }
}