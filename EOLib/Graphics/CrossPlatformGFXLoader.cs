// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using PELoaderLib;

namespace EOLib.Graphics
{
	public sealed class CrossPlatformGFXLoader : INativeGraphicsLoader
	{
		private readonly Dictionary<GFXTypes, IPEFile> _modules;

		public CrossPlatformGFXLoader()
		{
			_modules = new Dictionary<GFXTypes, IPEFile>();
			var values = (GFXTypes[])Enum.GetValues(typeof(GFXTypes));
			foreach (var gfx in values)
			{
				_modules.Add(gfx, LoadGFXFile(gfx));
			}
		}

		public Bitmap LoadGFX(GFXTypes file, int resourceValue)
		{
			var fileBytes = _modules[file].GetEmbeddedBitmapResourceByID(resourceValue + 100);

			if (fileBytes.Length == 0)
				throw new GFXLoadException(resourceValue, file);

			var ms = new MemoryStream(fileBytes);
			return (Bitmap)Image.FromStream(ms);
		}

		private IPEFile LoadGFXFile(GFXTypes file)
		{
			var number = ((int)file).ToString("D3");
			var fName = Path.Combine("gfx", "gfx" + number + ".egf");

			var peFile = new PEFile(fName);
			try
			{
				peFile.Initialize();
			}
			catch (IOException)
			{
				throw new LibraryLoadException(number, file);
			}

			if (!peFile.Initialized)
			{
				throw new LibraryLoadException(number, file);
			}

			return peFile;
		}

		~CrossPlatformGFXLoader()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var file in _modules.Values)
				{
					file.Dispose();
				}
				_modules.Clear();
			}

		}
	}
}
