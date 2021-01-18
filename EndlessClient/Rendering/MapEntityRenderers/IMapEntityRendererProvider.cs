using System;
using System.Collections.Generic;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public interface IMapEntityRendererProvider : IDisposable
    {
        IMapEntityRenderer GroundRenderer { get; }

        IReadOnlyList<IMapEntityRenderer> BaseRenderers { get; }

        IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get; }
    }
}
