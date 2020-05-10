namespace EOBot
{
    interface IBotFactory
    {
        IBot CreateBot(int index, string host, ushort port);
    }
}