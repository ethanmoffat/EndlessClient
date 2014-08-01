using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EndlessClient
{
	
	public class Config
	{
		[DllImport("kernel32.dll")]
		private static extern long GetVolumeInformation(string PathName, StringBuilder VolumeNameBuffer, UInt32 VolumeNameSize, ref UInt32 VolumeSerialNumber, ref UInt32 MaximumComponentLength, ref UInt32 FileSystemFlags, StringBuilder FileSystemNameBuffer, UInt32 FileSystemNameSize);

		public static string GetHDDSerial()
		{
			string strDriveLetter = System.Windows.Forms.Application.UserAppDataPath[0] + ":\\";
			StringBuilder VolLabel = new StringBuilder(256); // Label
			uint serNum = 0;
			uint maxCompLen = 0;
			uint VolFlags = 0;
			StringBuilder FSName = new StringBuilder(256); // File System Name

			long Ret = GetVolumeInformation(strDriveLetter, VolLabel, (UInt32)VolLabel.Capacity, ref serNum, ref maxCompLen, ref VolFlags, FSName, (UInt32)FSName.Capacity);

			return Convert.ToString(serNum);
		}
	}
}
