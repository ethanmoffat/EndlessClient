// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOBot
{
    interface IBot : IDisposable
    {
        /// <summary>
        /// Event that is called when work for the Bot has been completed
        /// </summary>
        event Action WorkCompleted;

        /// <summary>
        /// Initialization logic for the bot instance. Called automatically by the framework.
        /// </summary>
        void Initialize();

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
