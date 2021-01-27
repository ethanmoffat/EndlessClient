using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Map
{
    public interface IMapRenderer : IGameComponent
    {
        void StartMapTransition();

        void StartEarthquake(byte strength);
    }
}
