// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Practices.Unity;

namespace EndlessClient
{
	public static class Program
	{
		private static IGameRunner _gameRunner;

		[STAThread]
		public static void Main()
		{
			using (var unityContainer = new UnityContainer())
			{
#if DEBUG
				_gameRunner = new DebugGameRunner(unityContainer);
#else
				_gameRunner = new ReleaseGameRunner(unityContainer);
#endif
				if (_gameRunner.SetupDependencies())
					_gameRunner.RunGame();
			}
		}
	}
}