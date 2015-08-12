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

			//try {
				using (EOGame.Instance)
				{
					EOGame.Instance.Run();
				}
			//} catch (Exception ex) {
			//	MessageBox.Show("An unhandled exception has caused the game to crash:\n  " + ex.Message, "Game crashed!");
			//}

			Logger.Close();
		}
	}
#endif
}

