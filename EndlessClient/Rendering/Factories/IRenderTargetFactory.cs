using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Factories
{
    public interface IRenderTargetFactory
    {
        RenderTarget2D CreateRenderTarget();
    }
}
