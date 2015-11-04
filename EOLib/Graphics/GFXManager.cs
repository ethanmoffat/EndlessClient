// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
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

		private readonly GraphicsDevice _device;

		public GFXManager(GraphicsDevice dev)
		{
			if (_device != null)
				throw new ArgumentException("The GFX loader has already been initialized once.");
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
			var number = ((int)file).ToString("D3");
			var fName = System.IO.Path.Combine(new[] { "gfx", "gfx" + number + ".egf" });

			var library = Win32.LoadLibrary(fName);

			if (library == IntPtr.Zero)
			{
				int err = Marshal.GetLastWin32Error();
				throw new Exception(string.Format("Error {1} when loading library {0}\n{2}", number, err, new System.ComponentModel.Win32Exception(err).Message));
			}

			var image = Win32.LoadImage(library, (uint)(100 + resourceVal), 0 /*IMAGE_BITMAP*/, 0, 0, 0x00008000 | 0x00002000 /*LR_DEFAULT*/);

			if (image == IntPtr.Zero)
			{
				throw new GFXLoadException(resourceVal, file);
			}
			Bitmap ret = Image.FromHbitmap(image);

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

			Win32.FreeLibrary(library);
			Win32.DeleteObject(image);
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
