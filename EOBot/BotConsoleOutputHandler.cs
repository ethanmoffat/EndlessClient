// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOBot
{
    class BotConsoleOutputHandler : IBotFrameworkOutputHandler
    {
        public void OutputBotInitializationFailed(string failMessage)
        {
            Console.WriteLine(failMessage);
        }

        public void OutputBotInitializationSucceeded(int botIndex)
        {
            Console.WriteLine("Bot {0} initialized.", botIndex);
        }

        public void OutputAllBotsAreRunning(bool waitingForTermination)
        {
            Console.WriteLine("\nBot framework run has started. {0}\n",
                waitingForTermination ? "Waiting for CTRL+C" : "Application will terminate when all bots finish running.");
        }

        public void OutputWarnSomeBotsFailed()
        {
            Console.WriteLine("Some bot instances failed to initialize. These bot instances will not be run.");
        }
    }
}
