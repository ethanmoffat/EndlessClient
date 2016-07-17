// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOBot
{
    interface IBotFrameworkOutputHandler
    {
        void OutputBotInitializationFailed(string failMessage);
        void OutputBotInitializationSucceeded(int botIndex);
        void OutputAllBotsAreRunning(bool waitingForTermination);
        void OutputWarnSomeBotsFailed();
    }
}
