
using System.Runtime.InteropServices;

namespace EOBot
{
	internal static class Win32
	{
		public delegate bool ConsoleCtrlDelegate(CtrlTypes ctrlType);

		public enum CtrlTypes : uint
		{
			Ctrl_C = 0,
			Ctrl_Break,
			Close_Console,
			Logoff = 5,
			Shutdown
		}

		[DllImport("kernel32.dll")]
		public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);
	}
}