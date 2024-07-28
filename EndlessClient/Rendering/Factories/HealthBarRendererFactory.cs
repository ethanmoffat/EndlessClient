using AutomaticTypeMapper;
using EOLib.Graphics;

namespace EndlessClient.Rendering.Factories;

[AutoMappedType]
public class HealthBarRendererFactory : IHealthBarRendererFactory
{
    private readonly INativeGraphicsManager _nativeGraphicsManager;

    public HealthBarRendererFactory(INativeGraphicsManager nativeGraphicsManager)
    {
        _nativeGraphicsManager = nativeGraphicsManager;
    }

    public IHealthBarRenderer CreateHealthBarRenderer(IMapActor parentReference)
    {
        return new HealthBarRenderer(_nativeGraphicsManager, parentReference);
    }
}

public interface IHealthBarRendererFactory
{
    IHealthBarRenderer CreateHealthBarRenderer(IMapActor entity);
}