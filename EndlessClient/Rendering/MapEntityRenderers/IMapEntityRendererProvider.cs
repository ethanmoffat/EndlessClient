using System;
using System.Collections.Generic;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public interface IMapEntityRendererProvider
    {
        IMapEntityRenderer GroundRenderer { get; }

        IReadOnlyList<IMapEntityRenderer> BaseRenderers { get; }

        IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get; }
    }
}
