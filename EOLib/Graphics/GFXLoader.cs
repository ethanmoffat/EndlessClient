// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace EOLib.Graphics
{
	public class LibraryLoadException : Exception
	{
		public LibraryLoadException(string libraryNumber)
			: base(string.Format("Error {1} when loading library {0}\n{2}",
				libraryNumber,
				Marshal.GetLastWin32Error(),
				new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message))
		{
		}
	}

	public class GFXLoader : INativeGraphicsLoader
	{
		public Bitmap LoadGFX(GFXTypes file, int resourceVal)
		{
			var library = LoadLibraryModule(file);
			var image = LoadLibraryImage(file, library, resourceVal);
			
			Bitmap ret = Image.FromHbitmap(image);

			Win32.FreeLibrary(library);
			Win32.DeleteObject(image);

			return ret;
		}

		private IntPtr LoadLibraryModule(GFXTypes file)
		{
			var number = ((int)file).ToString("D3");
			var fName = System.IO.Path.Combine("gfx", "gfx" + number + ".egf");

			var library = Win32.LoadLibrary(fName);

			if (library == IntPtr.Zero)
				throw new LibraryLoadException(number);

			return library;
		}

		private IntPtr LoadLibraryImage(GFXTypes file, IntPtr library, int resourceVal)
		{
			var image = Win32.LoadImage(library, (uint)(100 + resourceVal), 0 /*IMAGE_BITMAP*/, 0, 0, 0x00008000 | 0x00002000 /*LR_DEFAULT*/);

			if (image == IntPtr.Zero)
			{
				throw new GFXLoadException(resourceVal, file);
			}

			return image;
		}
	}
}
