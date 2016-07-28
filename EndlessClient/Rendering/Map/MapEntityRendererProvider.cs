// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EndlessClient.Rendering.MapEntityRenderers;

namespace EndlessClient.Rendering.Map
{
    public class MapEntityRendererProvider : IMapEntityRendererProvider
    {
        private readonly List<IMapEntityRenderer> _renderers;

        public IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get { return _renderers; } }

        public MapEntityRendererProvider()
        {
            _renderers = new List<IMapEntityRenderer>();

            //todo: build up the renderers list
        }
    }
}