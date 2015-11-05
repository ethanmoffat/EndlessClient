// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

#include "EOCLI.h"

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "gdi32.lib")

using namespace System::IO;
using namespace msclr::interop;

namespace EOCLI
{
	Bitmap^ GFXLoaderCLI::LoadGFX(GFXTypes file, int resourceVal)
	{
		auto library = LoadLibraryModule(file);
		auto image = LoadLibraryImage(file, library, resourceVal);
		IntPtr imagePtr(image);

		auto retVal = Bitmap::FromHbitmap(imagePtr);

		::FreeLibrary((HMODULE)library);
		::DeleteObject(image);

		return retVal;
	}

	HANDLE GFXLoaderCLI::LoadLibraryModule(GFXTypes file)
	{
		auto number = static_cast<int>(file).ToString("D3");
		auto fName = Path::Combine("gfx", "gfx" + number + ".egf");

		marshal_context context;
		HANDLE library = ::LoadLibrary(context.marshal_as<LPCTSTR>(fName));

		if (!library || library == INVALID_HANDLE_VALUE)
			throw gcnew LibraryLoadException(number);

		return library;
	}

	HANDLE GFXLoaderCLI::LoadLibraryImage(GFXTypes file, HANDLE library, int resourceVal)
	{
		HANDLE image = ::LoadImage((HINSTANCE)library, MAKEINTRESOURCE(100 + resourceVal), IMAGE_BITMAP, 0, 0, LR_CREATEDIBSECTION | LR_SHARED);

		if (!image || image == INVALID_HANDLE_VALUE)
			throw gcnew GFXLoadException(resourceVal, file);

		return image;
	}
}