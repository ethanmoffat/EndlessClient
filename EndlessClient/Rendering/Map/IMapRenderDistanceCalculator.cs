using EOLib.IO.Map;

namespace EndlessClient.Rendering.Map
{
    public interface IMapRenderDistanceCalculator
    {
        MapRenderBounds CalculateRenderBounds(EOLib.Domain.Character.Character character, IMapFile currentMap);
    }
}