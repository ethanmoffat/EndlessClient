using System.Drawing;

namespace EOLib.Graphics
{
    public interface INativeGraphicsLoader
    {
        Bitmap LoadGFX(GFXTypes file, int resourceValue);
    }
}
