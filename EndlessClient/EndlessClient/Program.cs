using System;
using System.Windows.Forms;

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
			//try
			//{
				using (EOGame.Instance)
				{
					EOGame.Instance.Run();
				}
			//}
			//catch (Exception ex)
			//{
			//	Logger.Log("UNHANDLED EXCEPTION WAS ENCOUNTERED========================================");
			//	Logger.Log("Exception message: {0}", ex.Message);
			//	Logger.Log("Exception type:    {0}", ex.GetType().ToString());
			//	Logger.Log("Call Stack:\n{0}", ex.StackTrace);

			//	MessageBox.Show(
			//		"An unhandled exception has caused the game to crash. Debug builds will log this information in the log folder.",
			//		"Game crashed!");
			//}

			Logger.Close();
		}
	}
#endif
}

