using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using AutomaticTypeMapper;

namespace EOLib
{
    [AutoMappedType]
    public class HDSerialNumberServiceWindows : IHDSerialNumberService
    {
        public string GetHDSerialNumber()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return GetHDSerialNumberLinux();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return GetHDSerialNumberOSX();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetHDSerialNumberWindows();

            throw new NotImplementedException("HD serial number is not implemented on your platform");
        }

        private static string GetHDSerialNumberWindows()
        {
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
                (uint) fileSystemName.Capacity) != 0
                    ? Convert.ToString(serNum)
                    : string.Empty;
        }

        private static string GetHDSerialNumberLinux()
        {
            // use lsblk --nodeps -o serial to get serial number
            try
            {
                var serialNumber = "";

                var p = new ProcessStartInfo
                {
                    Arguments = "--nodeps -o serial",
                    CreateNoWindow = true,
                    FileName = "lsblk",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                using (var process = Process.Start(p))
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        serialNumber = output.Split('\n').Skip(1).Where(x => !string.IsNullOrEmpty(x)).First();
                    }
                }

                // remove non-numeric characters so eoserv can handle it
                serialNumber = Regex.Replace(serialNumber, @"\D", string.Empty);

                // make the serial number shorted so eoserv can handle it
                while (ulong.TryParse(serialNumber, out var sn) && sn > uint.MaxValue)
                    serialNumber = serialNumber.Substring(0, serialNumber.Length - 1);

                return serialNumber;
            }
            catch
            {
                // if this fails for ANY reason, just give a dummy value.
                // this isn't important enough to be worth crashing or notifying the user.

                return "12345"; // Just like my luggage
            }
        }

        private static string GetHDSerialNumberOSX()
        {
            try
            {
                var serialNumber = "";

                var p = new ProcessStartInfo
                {
                    Arguments = "-c IOPlatformExpertDevice -d 2",
                    CreateNoWindow = true,
                    FileName = "ioreg",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                using (var process = Process.Start(p))
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        serialNumber = output.Split('\n').Where(x => x.Contains("IOPlatformSerialNumber")).First();
                        var matches = Regex.Matches(serialNumber, "\"([^\"]*)\"");
                        serialNumber = matches[1].Value;
                        serialNumber = serialNumber.Substring(1, serialNumber.Length - 2);
                    }
                }

                // replace letters with their corresponding numbers
                serialNumber = Regex.Replace(serialNumber, "[A-Z]", m => ((int) m.Value[0] - 55).ToString());

                // make the serial number shorted so eoserv can handle it
                while (ulong.TryParse(serialNumber, out var sn) && sn > uint.MaxValue)
                    serialNumber = serialNumber.Substring(0, serialNumber.Length - 1);

                return serialNumber;
            }
            catch
            {
                // if this fails for ANY reason, just give a dummy value.
                // this isn't important enough to be worth crashing or notifying the user.

                return "12345"; // Just like my luggage
            }
        }
    }
}
