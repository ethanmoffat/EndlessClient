using System;

namespace EOBot
{
    public class BotException : Exception
    {
        public BotException(string message) : base(message) { }
    }
}
