// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Runtime.InteropServices;

namespace EOLib.Graphics
{
	public class LibraryLoadException : Exception
	{
		public GFXTypes WhichGFX { get; private set; }

		public LibraryLoadException(string libraryNumber, GFXTypes which)
			: base(string.Format("Error {1} when loading library {0}\n{2}",
				libraryNumber,
				Marshal.GetLastWin32Error(),
				new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message))
		{
			WhichGFX = which;
		}
	}
}
