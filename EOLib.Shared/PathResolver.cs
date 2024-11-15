using System.IO;
using System.Runtime.InteropServices;
using System;

namespace EOLib.Shared
{
    public static class PathResolver
    {
        public const string LocalFilesRoot = ".endlessclient";
        public static string ResourcesRoot { get; } = Path.Combine("Contents", "Resources");

        public static string GetPath(string inputPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine(ResourcesRoot, inputPath);
            }
            else
            {
                return inputPath;
            }
        }

        public static string GetModifiablePath(string inputPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                if (home != null)
                {
                    return Path.Combine(home, LocalFilesRoot, inputPath);
                }
            }

            return inputPath;
        }
    }
}
