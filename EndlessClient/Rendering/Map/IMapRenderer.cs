using EOLib.Domain.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Map
{
    public interface IMapRenderer : IGameComponent
    {
        MapCoordinate GridCoordinates { get; }

        bool MouseOver { get; }

        void StartMapTransition();

        void StartEarthquake(byte strength);

        void RedrawGroundLayer();

        void RenderEffect(byte x, byte y, short effectId);

        void ClearTransientRenderables();
    }
}
