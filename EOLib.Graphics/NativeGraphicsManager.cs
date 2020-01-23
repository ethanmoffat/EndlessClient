// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AutomaticTypeMapper;
using Microsoft.Xna.Framework.Graphics;
using Color = System.Drawing.Color;

namespace EOLib.Graphics
{
    [MappedType(BaseType = typeof(INativeGraphicsManager), IsSingleton = true)]
    public sealed class NativeGraphicsManager : INativeGraphicsManager
    {
        private readonly Dictionary<LibraryGraphicPair, Texture2D> _cache;
        private readonly object __cachelock__ = new object();

        private readonly INativeGraphicsLoader _gfxLoader;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;

        public NativeGraphicsManager(INativeGraphicsLoader gfxLoader, IGraphicsDeviceProvider graphicsDeviceProvider)
        {
            _cache = new Dictionary<LibraryGraphicPair, Texture2D>();
            _gfxLoader = gfxLoader;
            _graphicsDeviceProvider = graphicsDeviceProvider;
        }

        public Texture2D TextureFromResource(GFXTypes file, int resourceVal, bool transparent = false, bool reloadFromFile = false)
        {
            Texture2D ret;

            var key = new LibraryGraphicPair((int)file, 100 + resourceVal);

            lock (__cachelock__)
            {
                if (!reloadFromFile && _cache.ContainsKey(key))
                {
                    return _cache[key];
                }

                if (_cache.ContainsKey(key) && reloadFromFile)
                {
                    if (_cache[key] != null) _cache[key].Dispose();
                    _cache.Remove(key);
                }
            }

            using (var mem = new System.IO.MemoryStream())
            {
                using (var i = BitmapFromResource(file, resourceVal, transparent))
                    i.Save(mem, ImageFormat.Png);

                ret = Texture2D.FromStream(_graphicsDeviceProvider.GraphicsDevice, mem);
            }

            lock (__cachelock__)
            {
                _cache.Add(key, ret);
            }

            return ret;
        }

        private Bitmap BitmapFromResource(GFXTypes file, int resourceVal, bool transparent)
        {
            var ret = _gfxLoader.LoadGFX(file, resourceVal);

            if (transparent)
            {
                // TODO: 0x000000 is supposed to clip hair below it
                //       need to figure out how to clip this
                // if (file != GFXTypes.FemaleHat && file != GFXTypes.MaleHat)
                CrossPlatformMakeTransparent(ret, Color.Black);

                // for hats: 0x080000 is transparent
                if (file == GFXTypes.FemaleHat || file == GFXTypes.MaleHat)
                {
                    CrossPlatformMakeTransparent(ret, Color.FromArgb(0xFF, 0x08, 0x00, 0x00));
                }
            }

            return ret;
        }

        private static void CrossPlatformMakeTransparent(Bitmap bmp, Color transparentColor)
        {
            bmp.MakeTransparent(transparentColor);

#if LINUX
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

            var bmpBytes = new byte[bmp.Height * bmpData.Stride];
            Marshal.Copy(bmpData.Scan0, bmpBytes, 0, bmpBytes.Length);

            for (int i = 0; i < bmpBytes.Length; i += 4)
            {
                if (bmpBytes[i] == 0 && bmpBytes[i + 1] == 0 && bmpBytes[i + 2] == 0)
                    bmpBytes[i + 3] = 0;
            }

            Marshal.Copy(bmpBytes, 0, bmpData.Scan0, bmpBytes.Length);

            bmp.UnlockBits(bmpData);
#endif
        }

        public void Dispose()
        {
            lock (__cachelock__)
            {
                foreach (var text in _cache.Values)
                {
                    text.Dispose();
                }
                _cache.Clear();
            }
        }
    }

    [Serializable]
    public class GFXLoadException : Exception
    {
        public GFXLoadException(int resource, GFXTypes gfx)
            : base($"Unable to load graphic {resource + 100} from file gfx{(int) gfx:000}.egf") { }
    }
}
