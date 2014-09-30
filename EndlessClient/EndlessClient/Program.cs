using System;

namespace EndlessClient
{
#if WINDOWS
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
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

