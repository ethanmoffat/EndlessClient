using AutomaticTypeMapper;
using SixLabors.ImageSharp;
using System;

namespace EOLib.Graphics
{
    [MappedType(BaseType = typeof(INativeGraphicsLoader), IsSingleton = true)]
    public class NativeGraphicsLoader : INativeGraphicsLoader
    {
        private readonly IPEFileCollection _modules;

        public NativeGraphicsLoader(IPEFileCollection modules)
        {
            _modules = modules;
        }

        public IImage LoadGFX(GFXTypes file, int resourceValue)
        {
            var fileBytes = Array.Empty<byte>();
            try
            {
                fileBytes = _modules[file].GetEmbeddedBitmapResourceByID(resourceValue + 100);
            }
            catch (ArgumentException)
            {
#if DEBUG && THROW_ON_IMAGE_LOAD_FAIL
                throw;
#endif
            }

            if (fileBytes.Length == 0)
            {
#if DEBUG && THROW_ON_IMAGE_LOAD_FAIL
                throw new GFXLoadException(resourceValue, file);
#else
                return new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(1, 1);
#endif
            }

            return Image.Load(fileBytes);
        }
    }
}
