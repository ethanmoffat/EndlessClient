using AutomaticTypeMapper;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EOLib.Graphics
{
    [MappedType(BaseType = typeof(INativeGraphicsManager), IsSingleton = true)]
    public sealed class NativeGraphicsManager : INativeGraphicsManager
    {
        private readonly ConcurrentDictionary<GFXTypes, ConcurrentDictionary<int, Texture2D>> _cache;

        private readonly INativeGraphicsLoader _gfxLoader;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;

        public NativeGraphicsManager(INativeGraphicsLoader gfxLoader, IGraphicsDeviceProvider graphicsDeviceProvider)
        {
            _cache = new ConcurrentDictionary<GFXTypes, ConcurrentDictionary<int, Texture2D>>();
            _gfxLoader = gfxLoader;
            _graphicsDeviceProvider = graphicsDeviceProvider;
        }

        public Texture2D TextureFromResource(GFXTypes file, int resourceVal, bool transparent = false, bool reloadFromFile = false)
        {
            Texture2D ret;

            if (_cache.ContainsKey(file) && _cache[file].ContainsKey(resourceVal))
            {
                if (reloadFromFile)
                {
                    _cache[file][resourceVal]?.Dispose();
                    _cache[file].Remove(resourceVal, out _);
                }
                else
                {
                    return _cache[file][resourceVal];
                }
            }

            using (var i = BitmapFromResource(file, resourceVal, transparent))
            {
                if (!i.DangerousTryGetSinglePixelMemory(out var mem))
                {
                    using (var ms = new MemoryStream())
                    {
                        i.SaveAsPng(ms);
                        ret = Texture2D.FromStream(_graphicsDeviceProvider.GraphicsDevice, ms);
                    }
                }
                else
                {
                    ret = new Texture2D(_graphicsDeviceProvider.GraphicsDevice, i.Width, i.Height);
                    ret.SetData(mem.ToArray());
                }
            }

            if (_cache.ContainsKey(file) ||
                _cache.TryAdd(file, new ConcurrentDictionary<int, Texture2D>()))
            {
                _cache[file].TryAdd(resourceVal, ret);
            }

            return ret;
        }

        private Image<Rgba32> BitmapFromResource(GFXTypes file, int resourceVal, bool transparent)
        {
            var ret = (Image<Rgba32>)_gfxLoader.LoadGFX(file, resourceVal);

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
            foreach (var text in _cache.SelectMany(x => x.Value.Values))
                text.Dispose();

            _cache.Clear();
        }
    }

    [Serializable]
    public class GFXLoadException : Exception
    {
        public GFXLoadException(int resource, GFXTypes gfx)
            : base($"Unable to load graphic {resource + 100} from file gfx{(int) gfx:000}.egf") { }
    }
}
