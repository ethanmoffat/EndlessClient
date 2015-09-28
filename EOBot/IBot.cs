
using System;

namespace EOBot
{
	interface IBot : IDisposable
	{
		event Action WorkCompleted;

		void Initialize();
		void Run(bool waitForTermination);
		void Terminate();
	}
}
