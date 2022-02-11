using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

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
                    ((Image)i).Save(mem, new PngEncoder());

                ret = Texture2D.FromStream(_graphicsDeviceProvider.GraphicsDevice, mem);
            }

            lock (__cachelock__)
            {
                _cache.Add(key, ret);
            }

            return ret;
        }

        private IImage BitmapFromResource(GFXTypes file, int resourceVal, bool transparent)
        {
            var ret = (Image)_gfxLoader.LoadGFX(file, resourceVal);

            if (transparent)
            {
                // TODO: 0x000000 is supposed to clip hair below it
                //       need to figure out how to clip this
                // if (file != GFXTypes.FemaleHat && file != GFXTypes.MaleHat)
                CrossPlatformMakeTransparent(ret, Color.Black);

                // for hats: 0x080000 is transparent
                if (file == GFXTypes.FemaleHat || file == GFXTypes.MaleHat)
                {
                    CrossPlatformMakeTransparent(ret, Color.FromRgba(0x08, 0x00, 0x00, 0xFF));
                    CrossPlatformMakeTransparent(ret, Color.FromRgba(0x00, 0x08, 0x00, 0xFF));
                    CrossPlatformMakeTransparent(ret, Color.FromRgba(0x00, 0x00, 0x08, 0xFF));
                }
            }

            return ret;
        }

        private static void CrossPlatformMakeTransparent(Image bmp, Color transparentColor)
        {
            var brush = new RecolorBrush(transparentColor, Color.Transparent, 0.0001f);
            bmp.Mutate(x => x.Clear(brush));
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
