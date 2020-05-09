using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using AutomaticTypeMapper;

namespace EOLib
{
#if LINUX
    [AutoMappedType]
    public class HDSerialNumberServiceLinux : IHDSerialNumberService
    {
        public string GetHDSerialNumber()
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
    }
#endif
}
