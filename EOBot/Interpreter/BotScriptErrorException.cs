using System;

namespace EOBot.Interpreter
{
    public class BotScriptErrorException : Exception
    {
        public BotScriptErrorException(string message)
            : base(message) { }

        public BotScriptErrorException(string message, BotToken token)
            : base($"Error at line {token.LineNumber} column {token.Column}: {message}") { }
    }
}
