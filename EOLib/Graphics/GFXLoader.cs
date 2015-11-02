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

	public sealed class GFXLoader : IGraphicsLoader
	{
		private struct LibraryGraphicPair : IComparable
		{
			private readonly int LibraryNumber;
			private readonly int GraphicNumber;

			public LibraryGraphicPair(int lib, int gfx)
			{
				LibraryNumber = lib;
				GraphicNumber = gfx;
			}

			public int CompareTo(object other)
			{
				if (!(other is LibraryGraphicPair))
					return -1;

				LibraryGraphicPair rhs = (LibraryGraphicPair)other;

				if (rhs.LibraryNumber == LibraryNumber && rhs.GraphicNumber == GraphicNumber)
					return 0;

				return -1;
			}

			public override bool Equals(object obj)
			{
				if (!(obj is LibraryGraphicPair)) return false;
				LibraryGraphicPair other = (LibraryGraphicPair)obj;
				return other.GraphicNumber == GraphicNumber && other.LibraryNumber == LibraryNumber;
			}

			public override int GetHashCode()
			{
				return (LibraryNumber << 16) | GraphicNumber;
			}
		}

		private readonly Dictionary<LibraryGraphicPair, Texture2D> cache = new Dictionary<LibraryGraphicPair, Texture2D>();

		private readonly GraphicsDevice _device;

		public GFXLoader(GraphicsDevice dev)
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
