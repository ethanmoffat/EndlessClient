using EndlessClient.Rendering.Map;

namespace EndlessClient.Rendering.Factories
{
    public interface IMapRendererFactory
    {
        IMapRenderer CreateMapRenderer();
    }
}
