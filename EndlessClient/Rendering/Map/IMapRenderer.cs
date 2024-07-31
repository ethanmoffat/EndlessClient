using EOLib.Domain.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Map
{
    public interface IMapRenderer : IGameComponent, IDrawable
    {
        MapCoordinate GridCoordinates { get; }

        bool MouseOver { get; }

        void StartMapTransition();

        void StartEarthquake(int strength);

        void RedrawGroundLayer();

        void RenderEffect(MapCoordinate location, int effectId);

        void AnimateMouseClick();

        void ClearTransientRenderables();
    }
}