using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Factories
{
    public interface IRenderTargetFactory
    {
        RenderTarget2D CreateRenderTarget();
        RenderTarget2D CreateRenderTarget(RenderTargetUsage renderTargetUsage);
        RenderTarget2D CreateRenderTarget(int width, int height);
        RenderTarget2D CreateRenderTarget(int width, int height, RenderTargetUsage renderTargetUsage);
    }
}
