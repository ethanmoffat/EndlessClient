
using System;
using System.Threading.Tasks;

namespace EOLib
{
#if !MONO
	public static class TaskFramework
	{
		public static async Task Delay(int timeToDelay)
		{
			await TaskEx.Delay(timeToDelay);
		}

		public static async Task Run(Action actionToRun)
		{
			await TaskEx.Run(actionToRun);
		}
	}
#endif

#if MONO
	public static class TaskFramework
	{
		public static async Task Delay(int timeToDelay)
		{
			await Task.Delay(timeToDelay);
		}

		public static async Task Run(Action actionToRun)
		{
			await Task.Run(actionToRun);
		}
	}
#endif
}
