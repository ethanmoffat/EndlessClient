using System;
using System.Threading;
using System.Threading.Tasks;

namespace EOBot;

public interface IBot
{
    /// <summary>
    /// Event that is called when work for the Bot has been completed
    /// </summary>
    event Action WorkCompleted;

    /// <summary>
    /// Initialization logic for the bot instance. Called automatically by the framework.
    /// </summary>
    Task InitializeAsync(string host, int port);

    /// <summary>
    /// Run logic for the bot instance. Called automatically by the framework.
    /// </summary>
    /// <param name="waitForTermination">True to wait until a call to Terminate() is made, false otherwise</param>
    Task RunAsync(CancellationToken cancellationToken);
}