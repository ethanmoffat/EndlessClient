// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Color = System.Drawing.Color;

namespace EOLib.Graphics
{
	[Serializable]
	public class GFXLoadException : Exception
	{
		public GFXLoadException(int resource, GFXTypes gfx)
			: base(string.Format("Unable to load graphic {0} from file gfx{1:000}.egf", resource + 100, (int)gfx)) { }
	}

	public sealed class GFXManager : INativeGraphicsManager
	{
		private readonly Dictionary<LibraryGraphicPair, Texture2D> cache = new Dictionary<LibraryGraphicPair, Texture2D>();

		private readonly INativeGraphicsLoader _gfxLoader;
		private readonly GraphicsDevice _device;

		public GFXManager(INativeGraphicsLoader gfxLoader, GraphicsDevice dev)
		{
			if(gfxLoader == null) throw new ArgumentNullException("gfxLoader");
			if (dev == null) throw new ArgumentNullException("dev");

			_gfxLoader = gfxLoader;
			_device = dev;
		}

		public Texture2D TextureFromResource(GFXTypes file, int resourceVal, bool transparent = false, bool reloadFromFile = false)
		{
			Texture2D ret;

			var key = new LibraryGraphicPair((int)file, 100 + resourceVal);
			if (!reloadFromFile && cache.ContainsKey(key))
			{
				return cache[key];
			}

			if (cache.ContainsKey(key) && reloadFromFile)
			{
				if (cache[key] != null) cache[key].Dispose();
				cache.Remove(key);
			}

			using (var mem = new System.IO.MemoryStream())
			{
				using (var i = BitmapFromResource(file, resourceVal, transparent))
					i.Save(mem, System.Drawing.Imaging.ImageFormat.Png);

				ret = Texture2D.FromStream(_device, mem);
			}

			//need to double-check that the key isn't already in the cache:
			//  	multiple threads can enter this method simultaneously
			//avoiding a lock because this method is used for every graphic
			if (!cache.ContainsKey(key))
				cache.Add(key, ret);

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

		/// <summary>
		/// Disposes all cached textures
		/// </summary>
		public void Dispose()
		{
			foreach (var text in cache.Values)
			{
				text.Dispose();
			}
			cache.Clear();
		}
	}
}
