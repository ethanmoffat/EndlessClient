// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Graphics;
using Color = System.Drawing.Color;

namespace EOLib.Graphics
{
    public sealed class NativeGraphicsManager : INativeGraphicsManager
    {
        private readonly Dictionary<LibraryGraphicPair, Texture2D> _cache;

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
            if (!reloadFromFile && _cache.ContainsKey(key))
            {
                return _cache[key];
            }

            if (_cache.ContainsKey(key) && reloadFromFile)
            {
                if (_cache[key] != null) _cache[key].Dispose();
                _cache.Remove(key);
            }

            using (var mem = new System.IO.MemoryStream())
            {
                using (var i = BitmapFromResource(file, resourceVal, transparent))
                    i.Save(mem, ImageFormat.Png);

                ret = Texture2D.FromStream(_graphicsDeviceProvider.GraphicsDevice, mem);
            }

            //need to double-check that the key isn't already in the cache:
            //      multiple threads can enter this method simultaneously
            //avoiding a lock because this method is used for every graphic
            if (!_cache.ContainsKey(key))
            {
                _cache.Add(key, ret);
            }
            else
            {
                ret.Dispose();
                ret = _cache[key];
            }

            return ret;
        }

        private Bitmap BitmapFromResource(GFXTypes file, int resourceVal, bool transparent)
        {
            var ret = _gfxLoader.LoadGFX(file, resourceVal);

            if (transparent)
            {
                if (file != GFXTypes.FemaleHat && file != GFXTypes.MaleHat)
                    ret.MakeTransparent(Color.Black);

                // for hats: 0x080000 is transparent, 0x000000 is supposed to clip hair below it
                if (file == GFXTypes.FemaleHat || file == GFXTypes.MaleHat)
                {
                    //ret.MakeTransparent(Color.Black);
                    ret.MakeTransparent(Color.FromArgb(0xFF, 0x08, 0x00, 0x00));
                }
            }

            return ret;
        }

        public void Dispose()
        {
            foreach (var text in _cache.Values)
            {
                text.Dispose();
            }
            _cache.Clear();
        }
    }

    [Serializable]
    public class GFXLoadException : Exception
    {
        public GFXLoadException(int resource, GFXTypes gfx)
            : base(string.Format("Unable to load graphic {0} from file gfx{1:000}.egf", resource + 100, (int)gfx)) { }
    }
}
