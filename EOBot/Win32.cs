
using System.Runtime.InteropServices;

namespace EOBot
{
	internal static class Win32
	{
		public delegate bool ConsoleCtrlDelegate(CtrlTypes ctrlType);

		public enum CtrlTypes : uint
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}

		[DllImport("kernel32.dll")]
		public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);
	}
}