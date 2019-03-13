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
            return (Bitmap)Image.FromStream(ms);
        }
    }
}
