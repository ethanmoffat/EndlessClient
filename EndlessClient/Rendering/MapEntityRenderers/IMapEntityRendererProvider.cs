using System;
using System.Collections.Generic;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public interface IMapEntityRendererProvider : IDisposable
    {
        IMapEntityRenderer GroundRenderer { get; }

        IMapEntityRenderer ItemRenderer { get; }

        IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get; }
    }
}
