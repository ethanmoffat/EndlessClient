using System;

namespace EndlessClient
{
#if WINDOWS
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
#if DEBUG
			//XNAControls.XNAControl.DrawOrderVisible = true;
#endif
			using (EOGame.Instance)
			{
				EOGame.Instance.Run();
			}
		}
	}
#endif
}

