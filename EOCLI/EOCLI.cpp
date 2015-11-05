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
	GFXLoaderCLI::GFXLoaderCLI()
	{
		m_libraries = gcnew Dictionary<GFXTypes, int>();
		GFXTypes dummy = (GFXTypes)0;
		auto libraries = Enum::GetValues(dummy.GetType());

		for each (auto library in libraries)
		{
			auto value = (int)LoadLibraryModule((GFXTypes)library);
			m_libraries->Add((GFXTypes)library, value);
		}
	}

	GFXLoaderCLI::~GFXLoaderCLI()
	{
		if (m_isDisposed)
			return;

		this->!GFXLoaderCLI();

		m_isDisposed = true;
	}

	GFXLoaderCLI::!GFXLoaderCLI()
	{
		for each (auto library in m_libraries->Values)
		{
			::FreeLibrary((HMODULE)library);
		}

		m_libraries->Clear();
		delete m_libraries;
	}

	Bitmap^ GFXLoaderCLI::LoadGFX(GFXTypes file, int resourceVal)
	{
		auto library = (HANDLE)m_libraries[file];
		auto image = LoadLibraryImage(file, library, resourceVal);
		IntPtr imagePtr(image);

		auto retVal = Bitmap::FromHbitmap(imagePtr);

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
			throw gcnew LibraryLoadException(number, file);

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