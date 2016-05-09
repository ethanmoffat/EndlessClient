// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.IO;
using System.Text;

namespace EOLib.IO.Services
{
	public class HDSerialNumberService : IHDSerialNumberService
	{
		public string GetHDSerialNumber()
		{
			//todo: follow advice here http://stackoverflow.com/a/23415273/2562283
			var strDriveLetter = DriveInfo.GetDrives()[0].Name;
			var volumeLabel = new StringBuilder(256);
			uint serNum = 0;
			uint maxCompLen = 0;
			uint VolFlags = 0;
			var fileSystemName = new StringBuilder(256);

			return Win32.GetVolumeInformation(
				strDriveLetter,
				volumeLabel,
				(uint) volumeLabel.Capacity,
				ref serNum,
				ref maxCompLen,
				ref VolFlags,
				fileSystemName,
				(uint) fileSystemName.Capacity) != 0 ? 
				Convert.ToString(serNum) : "";
		}
	}
}
