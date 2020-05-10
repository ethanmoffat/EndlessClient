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
