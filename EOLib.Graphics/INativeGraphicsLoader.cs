using System;

namespace EOLib.Graphics;

public interface INativeGraphicsLoader
{
    ReadOnlyMemory<byte> LoadGFX(GFXTypes file, int resourceValue);
}