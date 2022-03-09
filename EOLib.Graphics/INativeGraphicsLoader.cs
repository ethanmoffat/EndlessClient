using PELoaderLib;
using SixLabors.ImageSharp;

namespace EOLib.Graphics
{
    public interface INativeGraphicsLoader
    {
        IImage LoadGFX(GFXTypes file, int resourceValue);
    }
}
