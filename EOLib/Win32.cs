using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EOLib
{
	internal static class Win32
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern int GetVolumeInformation(string PathName, 
													   StringBuilder VolumeNameBuffer,
													   UInt32 VolumeNameSize, 
													   ref UInt32 VolumeSerialNumber, 
													   ref UInt32 MaximumComponentLength, 
													   ref UInt32 FileSystemFlags, 
													   StringBuilder FileSystemNameBuffer,
													   UInt32 FileSystemNameSize);
		
		//lpszName is a uint because egf files use MAKEINTRESOURCE which casts the uint resource value to a string pointer
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern IntPtr LoadImage(IntPtr hinst, uint lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr LoadLibrary(string lpFileName);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool FreeLibrary(IntPtr hModule);
		[DllImport("gdi32.dll")]
		internal static extern bool DeleteObject(IntPtr hObject);
	}
}
