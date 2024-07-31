namespace EOBot
{
    interface IBotFactory
    {
        IBot CreateBot(int index);
    }
}