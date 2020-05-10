using EOLib.Domain.Character;
using EOLib.IO.Map;

namespace EndlessClient.Rendering.Map
{
    public interface IMapRenderDistanceCalculator
    {
        MapRenderBounds CalculateRenderBounds(ICharacter character, IMapFile currentMap);
    }
}
