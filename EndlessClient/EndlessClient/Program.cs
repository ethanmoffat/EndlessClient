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
			using (EOGame game = new EOGame())
			{
				game.Run();
			}
		}
	}
#endif
}

