using System;
using System.Collections.Generic;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public interface IMapEntityRendererProvider : IDisposable
    {
        IReadOnlyList<IMapEntityRenderer> MapBaseRenderers { get; }

        IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get; }
    }
}
