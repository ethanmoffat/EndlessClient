// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Drawing;
using System.IO;
using AutomaticTypeMapper;

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

        public Bitmap LoadGFX(GFXTypes file, int resourceValue)
        {
            var fileBytes = _modules[file].GetEmbeddedBitmapResourceByID(resourceValue + 100);

            if (fileBytes.Length == 0)
                throw new GFXLoadException(resourceValue, file);

            var ms = new MemoryStream(fileBytes);

            var bm = (Bitmap)Image.FromStream(ms);
#if !LINUX
            var temp = new Bitmap(bm);
            for (int y = 0; y < bm.Height; y++)
            {
                for (int x = 0; x < bm.Width; x++)
                {
                    bm.SetPixel(x, y, temp.GetPixel(x, temp.Height - 1 - y));
                }
            }
#endif
            return bm;
        }
    }
}
