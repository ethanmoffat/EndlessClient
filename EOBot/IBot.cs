using System;
using System.Threading.Tasks;

namespace EOBot
{
    public interface IBot : IDisposable
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
        void Run(bool waitForTermination);
        
        /// <summary>
        /// Forcibly terminate a bot instance that is running or waiting for termination (ie regardless of state)
        /// </summary>
        void Terminate();
    }
}
