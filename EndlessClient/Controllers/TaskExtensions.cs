using System.Threading.Tasks;
using EndlessClient.Rendering;

namespace EndlessClient.Controllers
{
    public static class TaskExtensions
    {
        public static Task ThrowIfFaulted(this Task t)
        {
            if (t.IsFaulted)
            {
                // Invoke any exception on the main game thread
                // Exceptions thrown by tasks are quietly swallowed
                // Invoking this on the main thread ensures the Update() loop catches it and
                //   handles it in the global exception handler (see EndlessGame::Update)
                Task.Run(DispatcherGameComponent.InvokeAsync(() => throw t.Exception).RunSynchronously);
            }

            return t;
        }
    }
}
