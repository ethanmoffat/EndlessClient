// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Map;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public interface IMapEntityRenderer
    {
        MapRenderLayer RenderLayer { get; }

        //todo: add required parameters for Render()
        void RenderElementAt(int row, int col);
    }
}
