using System;
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
    }
}
