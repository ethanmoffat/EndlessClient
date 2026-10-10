using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EOBot.Interpreter
{
    public class BotScriptErrorException : Exception
    {
        public bool HasLocation { get; }

        public BotScriptErrorException(string message)
            : base(message) { }

        public BotScriptErrorException(string message, BotToken token)
            : base(GetMessage(message, token)) => HasLocation = true;

        public BotScriptErrorException(string message, BotToken token, IEnumerable<(string, BotToken, int)> stack)
            : base(GetMessage(message, token, stack)) => HasLocation = true;

        private static string GetMessage(string message, BotToken token) => $"Error at line {token.LineNumber} column {token.Column}: {message}";

        private static string GetMessage(string message, BotToken token, IEnumerable<(string FuncName, BotToken Token, int ExecutionIndex)> stack)
        {
            if (!stack.Any())
                return GetMessage(message, token);

            var sb = new StringBuilder("at:\n");

            foreach (var frame in stack)
                sb.AppendLine($"    {frame.FuncName} (called from line {frame.Token.LineNumber})");

            sb.AppendLine();
            sb.Append(GetMessage(message, token));

            return sb.ToString();
        }
    }
}
