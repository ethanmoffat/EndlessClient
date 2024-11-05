using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EOLib.Graphics;

namespace EndlessClient.Rendering.Factories
{
    [AutoMappedType]
    public class HealthBarRendererFactory : IHealthBarRendererFactory
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        public HealthBarRendererFactory(IEndlessGameProvider endlessGameProvider,
                                        INativeGraphicsManager nativeGraphicsManager)
        {
            _endlessGameProvider = endlessGameProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
        }

        public IHealthBarRenderer CreateHealthBarRenderer(IMapActor parentReference)
        {
            return new HealthBarRenderer(_endlessGameProvider, _nativeGraphicsManager, parentReference);
        }
    }

    public interface IHealthBarRendererFactory
    {
        IHealthBarRenderer CreateHealthBarRenderer(IMapActor entity);
    }
}
